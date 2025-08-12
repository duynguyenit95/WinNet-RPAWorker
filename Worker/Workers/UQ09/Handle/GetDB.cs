using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQ09.Model;

namespace UQ09.Handle
{
    public class GetDB
    {
        private string _connectionString;
        private readonly Convert _convert = new Convert();
        public GetDB(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ORPConnection");
        }
        public (int,List<MaterialProduction>) GetDetail(string area)
        {
            var result = new List<MaterialProduction>();
            int productionId = 0;
            int weekId = _convert.GetWeekId();
            string sqlQuery = $"SELECT t0.*"
                            + $" FROM UQ09_ProductionDetail t0"
                            + $" INNER JOIN UQ09_Production t1"
                            + $" ON t0.ProductionId = t1.Id"
                            + $" WHERE t1.WeekId = {weekId}"
                            + $" AND t1.Area = '{area}'"
                            + $" AND t1.IsDone = 0";
            try
            {
                
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var detail = new MaterialProduction
                        {
                            DATA_TYPE_CD = "2",
                            PRD_MFCTRY_CD = rdr.GetString(rdr.GetOrdinal("PRD_MFCTRY_CD")),
                            SMPL_SPLR_MTRL_CD_PRD = rdr.GetString(rdr.GetOrdinal("SMPL_SPLR_MTRL_CD_PRD")),
                            SLS_CL_SPLR_CL_CD_PRD = rdr.GetString(rdr.GetOrdinal("SLS_CL_SPLR_CL_CD_PRD")),
                            SPLR_MTRL_CD_CNSMPTN = rdr.GetString(rdr.GetOrdinal("SPLR_MTRL_CD_CNSMPTN")),
                            SPLR_CL_CD_CNSMPTN = rdr.GetString(rdr.GetOrdinal("SPLR_CL_CD_CNSMPTN")),
                            PRD_WK = _convert.ConvertDate(rdr.GetDateTime(rdr.GetOrdinal("PRD_WK"))),
                            CNSMPTN_ACTL_QTY = _convert.ConvertNumber(rdr.GetDecimal(rdr.GetOrdinal("CNSMPTN_ACTL_QTY")))
                        };
                        result.Add(detail);
                        productionId = rdr.GetInt32(rdr.GetOrdinal("ProductionId"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (productionId, result);
        }

        public (int, List<MaterialPO>) GetMCDDetail(string area)
        {
            var result = new List<MaterialPO>();
            int mcdId = 0;
            int weekId = _convert.GetWeekId();
            string sqlQuery = $"SELECT t0.*"
                            + $" FROM UQ09_MCDDetail t0"
                            + $" INNER JOIN UQ09_MCD t1"
                            + $" ON t0.McdId = t1.Id"
                            + $" WHERE t1.WeekId = {weekId}"
                            + $" AND t1.Area = '{area}'"
                            + $" AND t1.IsDone = 0";
            try
            {

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var detail = new MaterialPO
                        {
                            MO_NUM = rdr.GetString(rdr.GetOrdinal("MO_NUM")),
                            MO_DTL_NUM = rdr.GetString(rdr.GetOrdinal("MO_DTL_NUM")).PadLeft(4, '0'),
                            MFCTRY_CD_SHIP_FROM = rdr.GetString(rdr.GetOrdinal("MFCTRY_CD_SHIP_FROM")),
                            MFCTRY_CD_SHIP_TO = rdr.GetString(rdr.GetOrdinal("MFCTRY_CD_SHIP_TO")),
                            TRNSPT_MODE = rdr.GetString(rdr.GetOrdinal("TRNSPT_MODE")),
                            SPLR_MTRL_CD = rdr.GetString(rdr.GetOrdinal("SPLR_MTRL_CD")),
                            SPLR_CL_CD = rdr.GetString(rdr.GetOrdinal("SPLR_CL_CD")),
                            DLVR_LCTN = rdr.GetString(rdr.GetOrdinal("DLVR_LCTN")),
                            SHIP_DLVR_DATE = _convert.ConvertDate(rdr.GetDateTime(rdr.GetOrdinal("SHIP_DLVR_DATE"))),
                            RCV_DLVR_DATE = _convert.ConvertDate(rdr.GetDateTime(rdr.GetOrdinal("RCV_DLVR_DATE"))),
                            MO_QTY = _convert.ConvertNumber(rdr.GetDecimal(rdr.GetOrdinal("MO_QTY"))),
                            MO_ISSD_DATE = _convert.ConvertDate(rdr.GetDateTime(rdr.GetOrdinal("MO_ISSD_DATE"))),
                            MO_STS_NAME = rdr.GetString(rdr.GetOrdinal("MO_STS_NAME"))
                        };
                        result.Add(detail);
                        mcdId = rdr.GetInt32(rdr.GetOrdinal("McdId"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (mcdId, result);
        }

        public bool MarkAsDone(int productionId)
        {
            string sqlQuery = $"UPDATE UQ09_Production SET IsDone = 1 WHERE Id = {productionId}";
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool MarkAsDoneMCD(int mcdId)
        {
            string sqlQuery = $"UPDATE UQ09_MCD SET IsDone = 1 WHERE Id = {mcdId}";
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
