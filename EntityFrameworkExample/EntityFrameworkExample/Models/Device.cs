

using System;
using System.Collections.Generic;

namespace EntityFrameworkExample.Models
{
    public partial class Device
    {
        public Device()
        {
            DeviceAction = new HashSet<DeviceAction>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Guid PublicId { get; set; }

        public ICollection<DeviceAction> DeviceAction { get; set; }
    }
}
