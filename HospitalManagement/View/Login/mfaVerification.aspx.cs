﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HospitalManagement.View.Login
{
    public partial class mfaVerification : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void mfaVerifyBtn_Click(object sender, EventArgs e)
        {
            var storedMfaCode = Session["MfaCode"]?.ToString();
            var username = Session["Username"]?.ToString();
            var userType = Session["UserType"]?.ToString();

            if (mfaCodeTxt.Text == storedMfaCode)
            {

                // Create an instance of LoginManager
                LoginManager userDataAccess = new LoginManager();

                // Create the authentication cookie
                userDataAccess.CreateAuthCookie(username, userType, HttpContext.Current);

                // Redirect to the appropriate dashboard
                userDataAccess.RedirectUser(userType, HttpContext.Current);

                // Clear the session
                 Session.Remove("MfaCode");
                 Session.Remove("Username");
                 Session.Remove("UserType");
            }
            else
            {
                Response.Write("<script>alert('Invalid MFA code.');</script>");
            }
        }
    }
}