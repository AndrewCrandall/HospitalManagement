﻿using HospitalManagement.Model; // Ensure the correct namespace for doctorManager
using HospitalManagement.Utilities; // Add namespace for InputValidator
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HospitalManagement.View.Doctor
{
    public partial class addNotes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if the session variables are null
            if (Session["Username"] == null || Session["UserType"] == null)
            {
                // Redirect to the login page or an error page
                Response.Redirect("~/View/Login.aspx");
            }
        }

        protected void SaveNotes_Click(object sender, EventArgs e)
        {
            try
            {
                // Sanitize input
                string firstName = InputValidator.SanitizeInput(patientFirstName.Text);
                string lastName = InputValidator.SanitizeInput(patientLastName.Text);
                string notesText = InputValidator.SanitizeInput(notes.Text);
                string appointmentDateText = InputValidator.SanitizeInput(appointmentDate.Text);

                // Get the username from the session
                string doctorUsername = Session["Username"]?.ToString();

                // Ensure the doctorUsername is valid
                if (string.IsNullOrEmpty(doctorUsername))
                {
                    throw new Exception("Doctor username is not found in the session.");
                }

                // Create an instance of doctorManager
                doctorManager doctor = new doctorManager();

                // Call the AddAppointment method
                bool isAdded = doctor.AddAppointment(doctorUsername, firstName, lastName, appointmentDateText, notesText);

                if (isAdded)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "Success", "alert('Appointment added successfully.');", true);
                    ClearFields(); // Optional: Clear the fields after successful addition
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "Failure", "alert('Failed to add appointment.');", true);
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Error", $"alert('An error occurred: {ex.Message}');", true);
            }
        }

        // Optional: Method to clear text fields
        private void ClearFields()
        {
            patientFirstName.Text = string.Empty;
            patientLastName.Text = string.Empty;
            notes.Text = string.Empty;
            appointmentDate.Text = string.Empty;
        }

        protected void Cancel_Click(object sender, EventArgs e)
        {
            // Clear all text fields
            ClearFields();
        }

        protected void Back_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/View/Doctor/doctorDashboard.aspx");

            // Clear the session
            Session.Clear();
        }
    }
}
