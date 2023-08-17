﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.ENTITIES.Models
{
    public class Category : BaseEntity
    {
        public Category()
        {
            Products = new List<Product>();
        }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        //Relational Properties

        public virtual List<Product> Products { get; set; }

    }
}
