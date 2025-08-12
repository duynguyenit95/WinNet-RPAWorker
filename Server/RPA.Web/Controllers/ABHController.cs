using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;

namespace RPA.Web.Controllers
{
    public class ABHController : Controller
    {
        [AllowAnonymous]
        [Route("abh/otp")]
        public async Task<IActionResult> GetAbhOTP()
        {
            var res = new OtpResponse
            {
                Result = false,
                Otp = string.Empty,
                Message = string.Empty
            };
            try
            {
                //string otp = await OutlookHelper.GetAbhOtp();
                res.Result = true;
                res.Otp = "";
                res.Message = "OTP retrieved successfully.";
            }
            catch (Exception ex)
            {
                res.Result = false;
                res.Message = ex.Message;
            }
            return Ok(res);
        }
        [AllowAnonymous]
        [Route("abh/introducer")]
        public async Task<IActionResult> GetAbhIntroducer()
        {
            var uncPath = "\\\\172.19.18.68\\Departments2\\HRA\\I-share\\NHÓM TUYỂN DỤNG-招聘组\\9. BÁO CÁO TỔNG HỢP  - 汇总报告\\Năm 2025 - 2025年\\Báo cáo tuyển dụng tháng 6 - 6月份\\7. Tổng hợp danh sách giới thiệu - 推荐明细.xlsx";
            if (!System.IO.File.Exists(uncPath))
            {
                return NotFound("File không tồn tại");
            }

            var stream = new FileStream(uncPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = Path.GetFileName(uncPath);

            return File(stream, contentType, fileName);
        }

        public class OtpResponse
        {
            public bool Result { get; set; }
            public string Otp { get; set; }
            public string Message { get; set; }
        }
    }
}
