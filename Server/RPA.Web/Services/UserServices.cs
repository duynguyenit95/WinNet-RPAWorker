using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RPA.Web.Data;

namespace RPA.Web.Services
{
    public interface IUserServices
    {
        string EmpID { get; }
        string FullName { get; }
        string LineNo { get; }
        string WorkShop { get; }
        string Factory { get; }
        void SetWorkshop(string workshop);
        void SetLine(string line);
        void SetFactory(string factory);
        Employee GetEmployee(string username, string password, bool isAdmin = false);
    }

    public class UserServices : IUserServices
    {
        private readonly UserContext userContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISession session;
        public UserServices(UserContext userContext, IHttpContextAccessor httpContextAccessor)
        {
            this.userContext = userContext;
            this.httpContextAccessor = httpContextAccessor;
            session = httpContextAccessor.HttpContext.Session;
        }
        public Employee GetEmployee(string username, string password, bool isAdmin = false)
        {
            return userContext.Employees.Where(x => x.EmpId.ToUpper() == username.ToUpper()
                                                            && (isAdmin || x.Password == password)).FirstOrDefault();
        }

        void IUserServices.SetWorkshop(string workshop)
        {
            session.SetString("WorkShop", workshop);
        }

        void IUserServices.SetLine(string line)
        {
            session.SetString("LineNo", line);
        }

        void IUserServices.SetFactory(string factory)
        {
            session.SetString("Factory", factory);
        }

        public string EmpID
        {
            get
            {
                var name = session.GetString("EmpID");
                if (string.IsNullOrEmpty(name))
                {
                    var emp = userContext.Employees.FirstOrDefault(x => x.EmpId == httpContextAccessor.HttpContext.User.Identity.Name) ?? new Employee();
                    session.SetString("EmpID", emp.EmpId);
                    name = emp.EmpId;
                }
                return name;
            }
        }
        public string FullName
        {
            get
            {
                var name = session.GetString("FullName");
                if (string.IsNullOrEmpty(name))
                {
                    var emp = userContext.Employees.FirstOrDefault(x => x.EmpId == httpContextAccessor.HttpContext.User.Identity.Name) ?? new Employee();
                    session.SetString("FullName", emp.EmpName);
                    name = emp.EmpName;
                }
                return name;
            }
        }
        public string WorkShop
        {
            get
            {
                var name = session.GetString("WorkShop");
                if (string.IsNullOrEmpty(name))
                {
                    var emp = userContext.Employees.FirstOrDefault(x => x.EmpId == httpContextAccessor.HttpContext.User.Identity.Name) ?? new Employee();
                    session.SetString("WorkShop", emp.Workshop);
                    name = emp.Workshop;
                }
                return name;
            }
        }
        public string LineNo
        {
            get
            {
                var name = session.GetString("LineNo");
                if (string.IsNullOrEmpty(name))
                {
                    var emp = userContext.Employees.FirstOrDefault(x => x.EmpId == httpContextAccessor.HttpContext.User.Identity.Name) ?? new Employee();
                    session.SetString("LineNo", emp.LineNo);
                    name = emp.LineNo;
                }
                return name;
            }
        }
        public string Factory
        {
            get
            {
                var name = session.GetString("Factory");
                if (string.IsNullOrEmpty(name))
                {
                    var emp = userContext.Employees.FirstOrDefault(x => x.EmpId == httpContextAccessor.HttpContext.User.Identity.Name) ?? new Employee();
                    session.SetString("Factory", emp.Factory);
                    name = emp.Factory;
                }
                return name;
            }
        }
    }
}
