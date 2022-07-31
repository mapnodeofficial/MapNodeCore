using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.DrinkToEarn
{
    public class CupItemViewModel
    {
        public int Id { get; set; } 
        public string Code { get; set; }

        public string ImageUrl { get; set; }

        public decimal HashRate { get; set; }

        public decimal MaxOut { get; set; }


        public decimal TimeToUse { get; set; }

        public decimal Price { get; set; }


        public string Name { get; set; }

        public bool IsInDrink { get; set; }

        public int CurrentHashRate { get; set; }

        public DateTime DateCreated { get;set;}
    }
}
