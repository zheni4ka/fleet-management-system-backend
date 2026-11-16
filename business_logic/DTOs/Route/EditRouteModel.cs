using business_logic.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace business_logic.DTOs
{
    public class EditRouteModel
    {
        public int Id { get; set; }
        public int StartLocationId { get; set; }
        public int DestinationLocationId { get; set; }
        public int AutoId { get; set; }
        public int DriverId { get; set; }
        public RouteStatus Status { get; set; }
    }
}
