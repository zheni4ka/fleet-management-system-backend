import http from "k6/http"
import { check, fail, sleep } from "k6"
import { Counter } from "k6/metrics"

const apiBaseUrl = __ENV.API_BASE_URL || "http://localhost:5196/api/v1"
const dispatcherUsername = __ENV.DISPATCHER_USERNAME || "dispatcher.kyiv"
const dispatcherPassword = __ENV.DISPATCHER_PASSWORD
const parallelRequestsCount = Number(__ENV.PARALLEL_REQUESTS || 20)
const synchronizedStartDelayInMilliseconds = 2000
const routeDurationInHours = 6

const createdRoutesCounter = new Counter("created_routes")
const assignmentConflictsCounter = new Counter("assignment_conflicts")
const unexpectedResponsesCounter = new Counter("unexpected_responses")

export const options = {
  scenarios: {
    conflictingRouteCreation: {
      executor: "per-vu-iterations",
      vus: parallelRequestsCount,
      iterations: 1,
      maxDuration: "1m",
    },
  },
  thresholds: {
    created_routes: ["count==1"],
    unexpected_responses: ["count==0"],
    "http_req_duration{name:createRoute}": ["max>=0"],
  },
}

function createJsonRequestParams(accessToken) {
  return {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
      "Accept-Language": "uk",
    },
  }
}

function getJson(apiPath, accessToken) {
  const apiResponse = http.get(
    `${apiBaseUrl}${apiPath}`,
    createJsonRequestParams(accessToken)
  )
  if (apiResponse.status !== 200) {
    fail(`GET ${apiPath} returned ${apiResponse.status}: ${apiResponse.body}`)
  }
  return apiResponse.json()
}

function formatIsoDateOnly(utcDateTime) {
  return utcDateTime.toISOString().slice(0, 10)
}

export function setup() {
  if (!dispatcherPassword) {
    fail("Set DISPATCHER_PASSWORD (the DemoSeed:DispatcherPassword value).")
  }

  const loginResponse = http.post(
    `${apiBaseUrl}/Account/login`,
    JSON.stringify({
      username: dispatcherUsername,
      password: dispatcherPassword,
    }),
    { headers: { "Content-Type": "application/json" } }
  )
  if (loginResponse.status !== 200) {
    fail(`Login as ${dispatcherUsername} returned ${loginResponse.status}.`)
  }
  const accessToken = loginResponse.json("accessToken")

  const availableAuto = getJson("/Auto/getAutoLookupOptions", accessToken).find(
    (autoLookupOption) => autoLookupOption.status === "Available"
  )
  if (availableAuto === undefined) {
    fail("The dispatcher has no available vehicle.")
  }
  const depotDriver = getJson("/Driver/getDriverLookupOptions", accessToken).find(
    (driverLookupOption) => driverLookupOption.depotId === availableAuto.depotId
  )
  if (depotDriver === undefined) {
    fail(`No driver shares depot ${availableAuto.depotId} with the vehicle.`)
  }
  const [startLocation, destinationLocation] = getJson(
    "/Location/getLocationLookupOptions",
    accessToken
  )

  const departureDayOffset = 400 + Math.floor(Math.random() * 3000)
  const departureTimeUtc = new Date(
    Date.UTC(
      new Date().getUTCFullYear(),
      new Date().getUTCMonth(),
      new Date().getUTCDate() + departureDayOffset,
      8,
      0,
      0
    )
  )
  const arrivalTimeUtc = new Date(
    departureTimeUtc.getTime() + routeDurationInHours * 3600 * 1000
  )

  console.log(
    `${parallelRequestsCount} parallel requests: vehicle ${availableAuto.number} (id ${availableAuto.id}), driver ${depotDriver.surname} (id ${depotDriver.id}), ${departureTimeUtc.toISOString()} – ${arrivalTimeUtc.toISOString()}`
  )

  return {
    accessToken,
    autoId: availableAuto.id,
    driverId: depotDriver.id,
    createRouteRequestBody: JSON.stringify({
      startLocationId: startLocation.id,
      destinationLocationId: destinationLocation.id,
      autoId: availableAuto.id,
      driverId: depotDriver.id,
      departureTimeUtc: departureTimeUtc.toISOString(),
      arrivalTimeUtc: arrivalTimeUtc.toISOString(),
      status: "Planned",
    }),
    departureDate: formatIsoDateOnly(departureTimeUtc),
    synchronizedStartAtMilliseconds:
      Date.now() + synchronizedStartDelayInMilliseconds,
  }
}

export default function (conflictingRouteCreationData) {
  const millisecondsUntilSynchronizedStart =
    conflictingRouteCreationData.synchronizedStartAtMilliseconds - Date.now()
  if (millisecondsUntilSynchronizedStart > 0) {
    sleep(millisecondsUntilSynchronizedStart / 1000)
  }

  const createRouteResponse = http.post(
    `${apiBaseUrl}/Route/createRoute`,
    conflictingRouteCreationData.createRouteRequestBody,
    {
      ...createJsonRequestParams(conflictingRouteCreationData.accessToken),
      tags: { name: "createRoute" },
    }
  )

  if (createRouteResponse.status === 201) {
    createdRoutesCounter.add(1)
  } else if (createRouteResponse.status === 409) {
    assignmentConflictsCounter.add(1)
  } else {
    unexpectedResponsesCounter.add(1)
    console.error(
      `Unexpected ${createRouteResponse.status}: ${createRouteResponse.body}`
    )
  }

  check(createRouteResponse, {
    "answered with 201 Created or 409 Conflict": (routeResponse) =>
      routeResponse.status === 201 || routeResponse.status === 409,
  })
}

export function teardown(conflictingRouteCreationData) {
  const routeSearchPath =
    `/Route/getRoutesPagedList?autoId=${conflictingRouteCreationData.autoId}` +
    `&departureFromDate=${conflictingRouteCreationData.departureDate}` +
    `&departureToDate=${conflictingRouteCreationData.departureDate}&page=1&pageSize=50`
  const persistedRoutesCount = getJson(
    routeSearchPath,
    conflictingRouteCreationData.accessToken
  ).totalCount

  console.log(
    `Routes stored for this vehicle and interval: ${persistedRoutesCount}`
  )
  check(persistedRoutesCount, {
    "exactly one route was stored": (routesCount) => routesCount === 1,
  })
}

export function handleSummary(summaryData) {
  const readCount = (metricName) =>
    summaryData.metrics[metricName] === undefined
      ? 0
      : summaryData.metrics[metricName].values.count
  const createRouteDuration =
    summaryData.metrics["http_req_duration{name:createRoute}"].values
  const summaryText = [
    "",
    "Conflicting route creation",
    `  parallel requests:   ${parallelRequestsCount}`,
    `  201 Created:         ${readCount("created_routes")}`,
    `  409 Conflict:        ${readCount("assignment_conflicts")}`,
    `  unexpected answers:  ${readCount("unexpected_responses")}`,
    `  median latency, ms:  ${createRouteDuration.med.toFixed(1)}`,
    `  p95 latency, ms:     ${createRouteDuration["p(95)"].toFixed(1)}`,
    `  invariant held:      ${readCount("created_routes") === 1 && readCount("unexpected_responses") === 0 ? "yes" : "NO"}`,
    "",
  ].join("\n")
  return { stdout: summaryText }
}
