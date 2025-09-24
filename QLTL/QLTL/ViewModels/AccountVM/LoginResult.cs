using QLTL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.ViewModels.AccountVM
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
    }

}