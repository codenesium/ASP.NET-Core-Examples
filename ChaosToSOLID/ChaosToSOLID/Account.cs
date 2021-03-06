﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chaos
{


    public class Account
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public string SSN { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created
        public string Token { get; set; } // secret token. We never want to expose this.

        public Account()
        {
        }
        public Account(int id, string name, string ssn, DateTime dateCreated, string token)
        {
            this.Id = id;
            this.Name = name;
            this.SSN = ssn;
            this.DateCreated = dateCreated;
            this.Token = token;
        }
    }
}
