﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

//we need these to talk to mysql
using MySql.Data;
using MySql.Data.MySqlClient;
//and we need this to manipulate data from a db
using System.Data;

namespace accountmanager
{
    /// <summary>
    /// Summary description for AccountServices
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class AccountServices : System.Web.Services.WebService
    {
        [WebMethod(EnableSession = true)]
        public string[] GetLikes()
        {
            string tmpStr = Likes(Session["id"].ToString());
            string[] likeArray = tmpStr.Split(',');
            return likeArray;
        }

        //EXAMPLE OF A SIMPLE SELECT QUERY (PARAMETERS PASSED IN FROM CLIENT)
        [WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
        public bool LogOn(string uid, string pass)
        {
            //we return this flag to tell them if they logged in or not
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            //NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
            string sqlSelect = "SELECT userID, count FROM users WHERE username=@idValue and password=@passValue";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //if we found an account, store the id and admin status in the session
                //so we can check those values later on other method calls to see if they 
                //are 1) logged in at all, and 2) and admin or not
                Session["id"] = sqlDt.Rows[0]["userID"];
                Session["index"] = sqlDt.Rows[0]["count"];
                //Session["admin"] = sqlDt.Rows[0]["admin"];
                success = true;
            }
            //return the result!
            return success;
        }
        

        [WebMethod(EnableSession = true)]
        public bool LogOff(int counter)
        {
            if(counter > 0)
            {
                counter = counter - 2;
            }
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //this is a simple update, with parameters to pass in values
            string sqlSelect = "update users set count=@indexValue where userID=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@indexValue", HttpUtility.UrlDecode(counter.ToString()));
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(Session["id"].ToString()));
            //sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
            //sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
            //sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
            //sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
            //if they log off, then we remove the session.  That way, if they access
            //again later they have to log back on in order for their ID to be back
            //in the session!
            
            Session.Abandon();
            return true;
        }
        [WebMethod(EnableSession = true)]
        public Image[] GetImage()
        {
            DataTable sqlDt = new DataTable("images");
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "select * from images";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);
            List<Image> images = new List<Image>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {


                images.Add(new Image
                {
                    picID = Convert.ToInt32(sqlDt.Rows[i]["picID"]),
                    keyWord = sqlDt.Rows[i]["keyWord"].ToString(),
                    image = sqlDt.Rows[i]["image"].ToString(),
                    likes = 0

                });


            }

            return images.ToArray();
        }
        [WebMethod]
        public string Likes(string id)
        {
            DataTable sqlDt = new DataTable("likes");
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "select liked from users where userID=@idValue";

            
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);
            string tmpStr = "";
            List<Image> images = new List<Image>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                tmpStr = sqlDt.Rows[i]["liked"].ToString();

                //images.Add(new Image
                //{
                //    picID = Convert.ToInt32(sqlDt.Rows[i]["picID"]),
                //    keyWord = sqlDt.Rows[i]["keyWord"].ToString(),
                //    image = sqlDt.Rows[i]["image"].ToString(),
                //    likes = 0

                //});


            }

            return tmpStr;
        }
        [WebMethod(EnableSession = true)]
        public int CurrentCount()
        {
            //DataTable sqlDt = new DataTable("CurrentCount");
            //string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //string sqlSelect = $"select count from users where userID={Session["id"]}";


            //MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //sqlDa.Fill(sqlDt);
            //int index = 0;

            //for (int i = 0; i < sqlDt.Rows.Count; i++)
            //{
            //    index = Convert.ToInt32(sqlDt.Rows[i]["count"]);



            //}
            return Convert.ToInt32(Session["index"]);

            //return index;
        }

        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        [WebMethod(EnableSession = true)]
        public void RequestAccount(string userName, string pass, string firstName, string lastName, string email)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into users (userID, username, password, Email, FirstName, LastName ) " +
                "values(null, @userValue, @passValue, @emailValue, @fnameValue,@lnameValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@userValue", HttpUtility.UrlDecode(userName));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
            sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e) {
            }
            sqlConnection.Close();
        }
        [WebMethod(EnableSession = true)]
        public void SavePicture(string picture)
        {
            //WRAPPING THE WHOLE THING IN AN IF STATEMENT TO CHECK IF THEY ARE AN ADMIN!

            string current = Likes(Session["id"].ToString());
            string final = current+","+picture;
           string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //this is a simple update, with parameters to pass in values
            string sqlSelect = "update users set liked=(@likeValue) where userID=@idValue";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@likeValue", HttpUtility.UrlDecode(final));
                sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(Session["id"].ToString()));
                //sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
                //sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
                //sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
                //sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

                sqlConnection.Open();
                //we're using a try/catch so that if the query errors out we can handle it gracefully
                //by closing the connection and moving on
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
                sqlConnection.Close();
            
        }

        //EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
        [WebMethod(EnableSession = true)]
		public Account[] GetAccounts()
		{
			//check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
			//just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
			//sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
			//Keeps everything simple.

			//WE ONLY SHARE ACCOUNTS WITH LOGGED IN USERS!
			if (Session["id"] != null)
			{
				DataTable sqlDt = new DataTable("users");

				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				string sqlSelect = "select userID, username, password, Email FirstName, LastName order by lastname";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				//gonna use this to fill a data table
				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				//filling the data table
				sqlDa.Fill(sqlDt);

				//loop through each row in the dataset, creating instances
				//of our container class Account.  Fill each acciount with
				//data from the rows, then dump them in a list.
				List<Account> accounts = new List<Account>();
				for (int i = 0; i < sqlDt.Rows.Count; i++)
				{
					//only share user id and pass info with admins!
					
					{
						accounts.Add(new Account
						{
							id = Convert.ToInt32(sqlDt.Rows[i]["userID"]),
							firstName = sqlDt.Rows[i]["FirstName"].ToString(),
							lastName = sqlDt.Rows[i]["LastName"].ToString(),
							email = sqlDt.Rows[i]["Email"].ToString()
						});
					}
				}
				//convert the list of accounts to an array and return!
				return accounts.ToArray();
			}
			else
			{
				//if they're not logged in, return an empty array
				return new Account[0];
			}
		}

		//EXAMPLE OF AN UPDATE QUERY WITH PARAMS PASSED IN
		









		//EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
		[WebMethod(EnableSession = true)]
		public Account[] GetAccountRequests()
		{//LOGIC: get all account requests and return them!
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				DataTable sqlDt = new DataTable("accountrequests");

				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				//requests just have active set to 0
				string sqlSelect = "select id, userid, pass, firstname, lastname, email from accounts where active=0 order by lastname";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDt);

				List<Account> accountRequests = new List<Account>();
				for (int i = 0; i < sqlDt.Rows.Count; i++)
				{
					accountRequests.Add(new Account
					{
						id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
						firstName = sqlDt.Rows[i]["firstname"].ToString(),
						lastName = sqlDt.Rows[i]["lastname"].ToString(),
						email = sqlDt.Rows[i]["email"].ToString()
					});
				}
				//convert the list of accounts to an array and return!
				return accountRequests.ToArray();
			}
			else {
				return new Account[0];
			}
		}

		//EXAMPLE OF A DELETE QUERY
		//[WebMethod(EnableSession = true)]
		//public void DeleteAccount(string id)
		//{
		//	if (Convert.ToInt32(Session["admin"]) == 1)
		//	{
		//		string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
		//		//this is a simple update, with parameters to pass in values
		//		string sqlSelect = "delete from accounts where id=@idValue";

		//		MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
		//		MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

		//		sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

		//		sqlConnection.Open();
		//		try
		//		{
		//			sqlCommand.ExecuteNonQuery();
		//		}
		//		catch (Exception e)
		//		{
		//		}
		//		sqlConnection.Close();
		//	}
		//}

		//EXAMPLE OF AN UPDATE QUERY
		[WebMethod(EnableSession = true)]
		public void ActivateAccount(string id)
		{
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				//this is a simple update, with parameters to pass in values
				string sqlSelect = "update accounts set active=1 where id=@idValue";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}

		//EXAMPLE OF A DELETE QUERY
		[WebMethod(EnableSession = true)]
		public void RejectAccount(string id)
		{
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				string sqlSelect = "delete from accounts where id=@idValue";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}


	}
}
