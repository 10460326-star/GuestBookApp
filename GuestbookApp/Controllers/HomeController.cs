using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

public class HomeController : Controller
{
    private string connString = "Server=LAPTOP-NAQM9HRP\\SQLEXPRESS;Database=GuestbookDB;Trusted_Connection=True;TrustServerCertificate=True;";

    // 1. 首頁：撈取資料（包含 Mood）
    public IActionResult Index(string searchString)
    {
        int? currentUserId = HttpContext.Session.GetInt32("CurrentUserId");
        if (currentUserId == null) return RedirectToAction("Login");

        var diaries = new List<dynamic>();

        using (SqlConnection conn = new SqlConnection(connString))
        {
            // 🚨 這裡多撈了 Mood 欄位
            string query = "SELECT Title, Content, CreateTime, Mood FROM Diaries WHERE UserId = @UserId";

            if (!string.IsNullOrEmpty(searchString))
            {
                query += " AND (Title LIKE @Search OR Content LIKE @Search)";
            }
            query += " ORDER BY CreateTime DESC";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                if (!string.IsNullOrEmpty(searchString))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + searchString + "%");
                }

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        diaries.Add(new
                        {
                            Title = reader["Title"].ToString(),
                            Content = reader["Content"].ToString(),
                            Time = ((DateTime)reader["CreateTime"]).ToString("yyyy-MM-dd HH:mm"),
                            Mood = reader["Mood"] != DBNull.Value ? reader["Mood"].ToString() : "" // 讀取 Mood
                        });
                    }
                }
            }
        }

        ViewBag.Username = HttpContext.Session.GetString("CurrentUsername");
        ViewBag.SearchString = searchString;
        ViewBag.Messages = diaries;
        return View();
    }

    // 2. 新增日記：精準寫入（包含 Mood）
    [HttpPost]
    public IActionResult AddMessage(string title, string content, string mood)
    {
        int? currentUserId = HttpContext.Session.GetInt32("CurrentUserId");
        if (currentUserId == null) return RedirectToAction("Login");

        // 如果前端沒傳心情，就給預設值 😊
        if (string.IsNullOrEmpty(mood)) mood = "😊";

        using (SqlConnection conn = new SqlConnection(connString))
        {
            // 🚨 SQL 語法加入 Mood 欄位與參數
            string query = "INSERT INTO Diaries (UserId, Title, Content, CreateTime, Mood) VALUES (@UserId, @Title, @Content, @CreateTime, @Mood)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Content", content);
                cmd.Parameters.AddWithValue("@CreateTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@Mood", mood); // 寫入單獨的心情欄位

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        return RedirectToAction("Index");
    }

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult DoLogin(string username, string password)
    {
        using (SqlConnection conn = new SqlConnection(connString))
        {
            string query = "SELECT UserId, Username FROM Users WHERE Username = @User AND Password = @Pass";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@User", username);
                cmd.Parameters.AddWithValue("@Pass", password);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        HttpContext.Session.SetInt32("CurrentUserId", (int)reader["UserId"]);
                        HttpContext.Session.SetString("CurrentUsername", reader["Username"].ToString());
                        return RedirectToAction("Index");
                    }
                }
            }
        }
        ViewBag.Error = "帳號或密碼錯誤！";
        return View("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}