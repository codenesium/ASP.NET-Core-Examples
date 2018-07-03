using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chaos
{
    public class Account
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public Guid GlobalCustomerId { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created

        public Account()
        {
        }
        public Account(int id, string name, Guid globalCustomerId, DateTime dateCreated)
        {
            this.Id = id;
            this.Name = name;
            this.GlobalCustomerId = globalCustomerId;
            this.DateCreated = dateCreated;
        }
    }


    public class Account2
    {
        public int Id { get; set; } // internal record identifier for this account
        public string Name { get; set; } // checking, saving, investment
        public Guid GlobalCustomerId { get; set; } // a unique customer id used to globally track a person's bank accounts
        public DateTime DateCreated { get; set; } // when the account was created
        public string Token { get; set; } // secret token. We never want to expose this.

        public Account2()
        {
        }
        public Account2(int id, string name, Guid globalCustomerId, DateTime dateCreated, string token)
        {
            this.Id = id;
            this.Name = name;
            this.GlobalCustomerId = globalCustomerId;
            this.DateCreated = dateCreated;
            this.Token = token;
        }
    }
}
