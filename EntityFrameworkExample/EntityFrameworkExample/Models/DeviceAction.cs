using System;
using System.Collections.Generic;

namespace EntityFrameworkExample.Models
{
    public partial class DeviceAction
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Device Device { get; set; }
    }
}
