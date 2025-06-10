using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bcommerce.Infrastructure.Data.Models
{
    public class BrandDataModel
    {
        public Guid brand_id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string? description { get; set; }
        public string? logo_url { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
