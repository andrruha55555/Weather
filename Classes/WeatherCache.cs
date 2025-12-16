using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using Weather.Models;

namespace Weather.Classes
{
    public class WeatherCache
    {
        private static string connectionString;
        public static readonly int DAILY_LIMIT = 50;
        private static readonly int CACHE_EXPIRE_MINUTES = 30; 

        static WeatherCache()
        {
            connectionString = ConfigurationManager.ConnectionStrings["WeatherCacheDB"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=127.0.0.1;Port=3306;Database=weather_cache;Uid=root;Pwd=;";
            }
        }
        public static async Task<DataResponse> GetCachedWeather(string cityName)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT response_data, request_count, 
                               TIMESTAMPDIFF(MINUTE, last_requested, NOW()) as minutes_passed
                        FROM weather_cache 
                        WHERE city_name = @city 
                        AND expires_at > NOW() 
                        ORDER BY created_at DESC 
                        LIMIT 1";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@city", cityName);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string json = reader.GetString("response_data");
                                int requestCount = reader.GetInt32("request_count");
                                int minutesPassed = reader.GetInt32("minutes_passed");

                                Console.WriteLine($"✓ Данные из кэша. Город: {cityName}");
                                Console.WriteLine($"  Запросов: {requestCount}, Прошло минут: {minutesPassed}");

                                await reader.CloseAsync();

                           
                                string updateQuery = @"
                                    UPDATE weather_cache 
                                    SET request_count = request_count + 1, 
                                        last_requested = NOW() 
                                    WHERE city_name = @city 
                                    AND expires_at > NOW()";

                                using (var updateCmd = new MySqlCommand(updateQuery, connection))
                                {
                                    updateCmd.Parameters.AddWithValue("@city", cityName);
                                    await updateCmd.ExecuteNonQueryAsync();
                                }

                                return JsonConvert.DeserializeObject<DataResponse>(json);
                            }
                            else
                            {
                                Console.WriteLine($"✗ Нет актуальных данных в кэше для: {cityName}");
                                Console.WriteLine($"  (данные устарели или отсутствуют)");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении из кэша: {ex.Message}");
            }

            return null;
        }

        public static async Task SaveToCache(string cityName, float? lat, float? lon, DataResponse data)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string deleteQuery = "DELETE FROM weather_cache WHERE city_name = @city";
                    using (var deleteCmd = new MySqlCommand(deleteQuery, connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@city", cityName);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    string insertQuery = @"
                        INSERT INTO weather_cache 
                        (city_name, latitude, longitude, response_data, created_at, expires_at, request_count, last_requested) 
                        VALUES (@city, @lat, @lon, @data, NOW(), DATE_ADD(NOW(), INTERVAL @minutes MINUTE), 1, NOW())";

                    using (var cmd = new MySqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@city", cityName);
                        cmd.Parameters.AddWithValue("@lat", lat.HasValue ? (object)lat.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@lon", lon.HasValue ? (object)lon.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(data));
                        cmd.Parameters.AddWithValue("@minutes", CACHE_EXPIRE_MINUTES);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine($"✓ Данные сохранены в кэш на {CACHE_EXPIRE_MINUTES} минут");
                    Console.WriteLine($"  Город: {cityName}");
                    Console.WriteLine($"  Истекает в: {DateTime.Now.AddMinutes(CACHE_EXPIRE_MINUTES):HH:mm:ss}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении в кэш: {ex.Message}");
            }
        }

        public static async Task<bool> CheckAndIncrementLimit(string userId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string today = DateTime.Now.ToString("yyyy-MM-dd");
                    string checkQuery = "SELECT request_count FROM request_limits WHERE user_id = @userId AND request_date = @date";
                    int currentCount = 0;

                    using (var checkCmd = new MySqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@userId", userId);
                        checkCmd.Parameters.AddWithValue("@date", today);

                        var result = await checkCmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            currentCount = Convert.ToInt32(result);
                        }
                    }
                    if (currentCount >= DAILY_LIMIT)
                    {
                        Console.WriteLine($"✗ Лимит исчерпан! Сегодня уже {currentCount}/{DAILY_LIMIT} запросов");
                        return false;
                    }
                    string updateQuery = @"
                        INSERT INTO request_limits (user_id, request_date, request_count, last_request) 
                        VALUES (@userId, @date, 1, NOW()) 
                        ON DUPLICATE KEY UPDATE 
                        request_count = request_count + 1, 
                        last_request = NOW()";

                    using (var updateCmd = new MySqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@userId", userId);
                        updateCmd.Parameters.AddWithValue("@date", today);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine($"✓ Запрос к API. Лимит: {currentCount + 1}/{DAILY_LIMIT}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке лимита: {ex.Message}");
                return true;
            }
        }
        public static async Task<(int todayCount, int remaining, int cachedCities, string cacheInfo)> GetStats(string userId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string today = DateTime.Now.ToString("yyyy-MM-dd");
                    int todayCount = 0;
                    int cachedCities = 0;
                    string cacheInfo = "";
                    string limitQuery = "SELECT request_count FROM request_limits WHERE user_id = @userId AND request_date = @date";
                    using (var limitCmd = new MySqlCommand(limitQuery, connection))
                    {
                        limitCmd.Parameters.AddWithValue("@userId", userId);
                        limitCmd.Parameters.AddWithValue("@date", today);

                        var result = await limitCmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            todayCount = Convert.ToInt32(result);
                        }
                    }
                    string cacheQuery = @"
                        SELECT 
                            COUNT(DISTINCT city_name) as city_count,
                            GROUP_CONCAT(CONCAT(city_name, ' (', TIMESTAMPDIFF(MINUTE, NOW(), expires_at), 'м)')) as cities
                        FROM weather_cache 
                        WHERE expires_at > NOW()";

                    using (var cacheCmd = new MySqlCommand(cacheQuery, connection))
                    {
                        using (var reader = await cacheCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                cachedCities = reader.GetInt32("city_count");
                                if (!reader.IsDBNull(reader.GetOrdinal("cities")))
                                {
                                    cacheInfo = reader.GetString("cities");
                                }
                            }
                        }
                    }

                    int remaining = Math.Max(0, DAILY_LIMIT - todayCount);
                    return (todayCount, remaining, cachedCities, cacheInfo);
                }
            }
            catch (Exception)
            {
                return (0, DAILY_LIMIT, 0, "");
            }
        }

        public static async Task CleanupOldCache()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string cleanupQuery = "DELETE FROM weather_cache WHERE expires_at < NOW()";
                    using (var cmd = new MySqlCommand(cleanupQuery, connection))
                    {
                        int deleted = await cmd.ExecuteNonQueryAsync();
                        if (deleted > 0)
                        {
                            Console.WriteLine($"Очищено {deleted} устаревших записей кэша");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке кэша: {ex.Message}");
            }
        }
    }
}