﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Web;
using System.Web.Security;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Data;
using System.Text;
public class LoginManager : SqlConnectionManager
{
    public LoginManager() : base() // Calls the base constructor
    {
    }

    public (bool isValid, string userType) ValidateUser(string username, string password)
    {
        try
        {
            OpenConnection();
            string query = "SELECT userType, password FROM Users WHERE username=@username;";

            using (SqlCommand command = new SqlCommand(query, GetConnection()))
            {
                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedUserType = reader["userType"].ToString();
                        string storedPassword = reader["password"].ToString();

                        // Verify the input password against the stored hashed password
                        if (StringHasher.VerifyString(password, storedPassword))
                        {
                            return (true, storedUserType);
                        }
                    }
                    return (false, null);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating user: {ex.Message}");
            return (false, null);
        }
        finally
        {
            CloseConnection();
        }
    }





    public string GenerateMfaCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // Generate a 6-digit code
    }


    public async Task SendMfaCodeViaEmail(string email, string mfaCode)
    {
        var fromAddress = new MailAddress("healthcarecsc4022@gmail.com", "Your App Name");
        var toAddress = new MailAddress(email);
        const string fromPassword = "wahg wyhj xobc swuk"; // Use the App Password 
        const string subject = "Your MFA Code";
        string body = $"Your MFA code is: {mfaCode}";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        try
        {
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                await smtp.SendMailAsync(message);
            }
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine($"SMTP Exception: {smtpEx.Message}");
            if (smtpEx.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {smtpEx.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
        }
    }

    public string GetEmailForMfa(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            // Log the invalid input scenario
            Console.WriteLine("Username is null or empty.");
            return null; // or throw an exception
        }

        try
        {
            OpenConnection();
            string query = "SELECT email FROM Users WHERE userID = (SELECT userID FROM Users WHERE username=@username);";

            using (SqlCommand command = new SqlCommand(query, GetConnection()))
            {
                // Use SqlDbType to specify the parameter type explicitly
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;

                var result = command.ExecuteScalar();
                return result?.ToString(); // Return email or null if not found
            }
        }
        catch (SqlException sqlEx)
        {
            // Log SQL-specific exceptions
            Console.WriteLine($"SQL Error retrieving email for MFA: {sqlEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            // Log general exceptions
            Console.WriteLine($"Error retrieving email for MFA: {ex.Message}");
            return null;
        }
        finally
        {
            CloseConnection();
        }
    }

    public void CreateAuthCookie(string username, string userType, HttpContext context)
    {
        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
            1,
            username,
            DateTime.Now,
            DateTime.Now.AddMinutes(130),
            false,
            userType,
            FormsAuthentication.FormsCookiePath);

        string encryptedTicket = FormsAuthentication.Encrypt(ticket);
        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
        {
            Path = "/",
            Secure = false, // Set to true for HTTPS
            HttpOnly = true
        };

        context.Response.Cookies.Add(authCookie);
    }

    public void RedirectUser(string userType, HttpContext context)
    {
        switch (userType)
        {
            case "Admin":
                context.Response.Redirect("~/View/Admin/adminDashboard.aspx");
                break;
            case "Doctor":
                context.Response.Redirect("~/View/Doctor/doctorDashboard.aspx");
                break;
            case "Patient":
                context.Response.Redirect("~/View/Patient/patientDashboard.aspx");
                break;
            default:
                context.Response.Write("<script>alert('Unknown user type.');</script>");
                break;
        }
    }
public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            // Convert the password into bytes
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert byte array to string
            StringBuilder builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}



    public void RegisterNewUser(string username, string password, string email, string firstName, string lastName)
    {
        // Hash the password
        string hashedPassword = StringHasher.HashString(password);
        string userType = "Patient"; // Hardcoded user type
        DateTime createdAt = DateTime.Now; // Capture the current date and time

        try
        {
            OpenConnection();
            string query = "INSERT INTO Users (username, password, email, firstName, lastName, userType, createdAt) " +
                           "VALUES (@username, @password, @email, @firstName, @lastName, @userType, @createdAt);";

            using (SqlCommand command = new SqlCommand(query, GetConnection()))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", hashedPassword);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@userType", userType); // Use the hardcoded value
                command.Parameters.AddWithValue("@createdAt", createdAt); // Set the createdAt value

                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
















    /*
    


    //DELETE LATER

    public void UpdatePasswords()
    {
        // Array of passwords to update
        var passwords = new[]
        {
        "password1", "password2", "password3", "password4", "password5",
        "passwordD1", "passwordD2", "passwordD3", "passwordD4", "passwordD5",
        "passwordP1", "passwordP2", "passwordP3", "passwordP4", "passwordP5"
    };

        var usernames = new[]
        {
        "admin1", "admin2", "admin3", "admin4", "admin5",
        "doctor1", "doctor2", "doctor3", "doctor4", "doctor5",
        "patient1", "patient2", "patient3", "patient4", "patient5"
    };

        try
        {
            OpenConnection();

            for (int i = 0; i < usernames.Length; i++)
            {
                string hashedPassword = StringHasher.HashString(passwords[i]);
                string query = "UPDATE Users SET password = @password WHERE username = @username;";

                using (SqlCommand command = new SqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@username", usernames[i]);
                    command.Parameters.AddWithValue("@password", hashedPassword);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        Console.WriteLine($"No user found with username: {usernames[i]}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /*
    public void EncryptAllNotes()
    {
        // Hardcoded appointment IDs and their corresponding notes
        var appointmentIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 1002, 2002 };
        var notes = new[]
        {
        "Annual checkup",
        "Skin rash consultation",
        "Neurological exam",
        "Pediatric checkup",
        "Orthopedic assessment",
        "test",
        "test delete later",
        "Test Delete Later",
        "test",
        "Blood work"
    };

        try
        {
            OpenConnection();

            for (int i = 0; i < appointmentIds.Length; i++)
            {
                // Encrypt the notes
                string encryptedNotes = StringHasher.Encrypt(notes[i]);

                // Update the notes in the database
                string updateQuery = "UPDATE Appointments SET notes = @notes WHERE appointmentID = @appointmentID;";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, GetConnection()))
                {
                    updateCommand.Parameters.AddWithValue("@notes", encryptedNotes);
                    updateCommand.Parameters.AddWithValue("@appointmentID", appointmentIds[i]);
                    updateCommand.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error encrypting notes: " + ex.Message);
        }
        finally
        {
            CloseConnection();
        }
    }
    */

}



