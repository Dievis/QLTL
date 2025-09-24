using QLTL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLTL.Helpers
{
    public static class CodeHelper
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// Sinh mã nhân viên duy nhất (ví dụ: BVUB123456)
        /// </summary>
        public static string GenerateEmployeeCode(QLTL_NEWEntities db)
        {
            string code;

            do
            {
                // BVUB + 6 chữ số ngẫu nhiên
                code = "BVUB" + _rnd.Next(100000, 999999);
            }
            while (db.Users.Any(u => u.EmployeeCode == code));

            return code;
        }
    }
}