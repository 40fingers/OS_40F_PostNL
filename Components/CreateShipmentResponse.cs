using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_40F_PostNL.Components
{
    public class CreateShipmentResponse
    {
        public List<ResponseShipment> ResponseShipments { get; set; } = new List<ResponseShipment>();
    }

    public class ResponseShipment
    {
        public string Barcode { get; set; }
        public List<ResponseLabel> Labels { get; set; } = new List<ResponseLabel>();

    }

    public class ResponseLabel
    {
        /// <summary>
        /// Base64 encoded file content
        /// </summary>
        public string Content { get; set; }
        public string Labeltype { get; set; }
        public string OutputType { get; set; }
    }
}
