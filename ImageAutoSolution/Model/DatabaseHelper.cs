using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace ImageAutoSolution.Model
{
    public class DatabaseHelper
    {
        public static readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=BCR_MOLE;Integrated Security=True; Encrypt=false;";

        public static List<string> GetGroupNames()
        {
            var result = new List<string>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT GroupName FROM T_DEVICE_INFO";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            return result;
        }

        public static List<ImageModel> GetImageData(int? triggerIndex = null, DateTime? startDate = null, DateTime? endDate = null, string groupName = null)
        {
            var result = new List<ImageModel>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                                    SELECT 
                                        t2.GroupName, 
                                        MIN(t1.CreateDT) AS CreateDT,
                                        Min(t1.Trigger_Index) AS Trigger_Index, 
                                        MAX(t1.Good_Read) AS Good_Read, 
                                        Min(t1.MacAddress) AS MacAddress,
                                        MIN(t1.Image_Index) AS Image_Index,
                                        MIN(t2.Name)  AS Name        
                                    FROM 
                                        T_FTP_Image t1
                                    INNER JOIN 
                                        T_DEVICE_INFO t2 
                                        ON t1.MacAddress = t2.MacAddress ";

                    var whereClauses = new List<string>();
                    if (triggerIndex.HasValue) whereClauses.Add("t1.Trigger_Index = @triggerIndex");
                    if (startDate.HasValue) whereClauses.Add("t1.CreateDT >= @startDate");
                    if (endDate.HasValue) whereClauses.Add("t1.CreateDT <= @endDate");

                    if (!string.IsNullOrEmpty(groupName) && groupName != "전체") whereClauses.Add("t2.GroupName = @groupName");

                    if (whereClauses.Any())
                        query += " WHERE " + string.Join(" AND ", whereClauses);


                    query += " GROUP BY t2.GroupName, t1.Trigger_Index";

                    var havingClauses = new List<string>();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (triggerIndex.HasValue)
                            command.Parameters.AddWithValue("@triggerIndex", triggerIndex.Value);
                        if (startDate.HasValue)
                            command.Parameters.AddWithValue("@startDate", startDate.Value);
                        if (endDate.HasValue)
                            command.Parameters.AddWithValue("@endDate", endDate.Value);
                        if (!string.IsNullOrEmpty(groupName) && groupName != "전체")
                            command.Parameters.AddWithValue("@groupName", groupName);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new ImageModel
                                {
                                    GroupName = reader.GetString(0),
                                    CreateDT = reader.GetDateTime(1),
                                    Trigger_Index = reader.GetInt32(2),
                                    Good_Read = reader.GetInt32(3),
                                    MacAddress = reader.GetString(4),
                                    Image_Index = reader.GetInt32(5),
                                    Name = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            return result;
        }

        public static List<ImageModel> GetImageDataByTriggerIndex(int triggerIndex)
        {
            var result = new List<ImageModel>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                                SELECT 
                                    t2.GroupName, 
                                    MIN(t1.CreateDT) AS CreateDT,
                                    t1.Trigger_Index, 
                                    MAX(t1.Good_Read) AS Good_Read, 
                                    MIN(t1.MacAddress) AS MacAddress,
                                    MIN(t1.Image_Index) AS Image_Index,
                                    Name
                                FROM 
                                    T_FTP_Image t1
                                INNER JOIN 
                                    T_DEVICE_INFO t2 
                                    ON t1.MacAddress = t2.MacAddress
                                WHERE 
                                    t1.Trigger_Index = @triggerIndex
                                GROUP BY 
                                    t2.GroupName, t1.Trigger_Index, Name";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@triggerIndex", triggerIndex);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new ImageModel
                                {

                                    GroupName = reader.GetString(0),
                                    CreateDT = reader.GetDateTime(1),
                                    Trigger_Index = reader.GetInt32(2),
                                    Good_Read = reader.GetInt32(3),
                                    MacAddress = reader.GetString(4),
                                    Image_Index = reader.GetInt32(5),
                                    Name = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

            return result;
        }

    }
}
