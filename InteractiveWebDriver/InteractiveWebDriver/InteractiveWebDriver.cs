using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using RestSharp;

namespace InteractiveWebDriver
{
    public static class InteractiveWebDriver
    {
        private static readonly string ServerUrl; 

        static InteractiveWebDriver()
        {
            ServerUrl = ConfigurationManager.AppSettings.Get("Common.InteractiveWebDriver.ServerUrl") ?? "http://localhost:4444/";
        }

        /// <summary>
        /// Creates a new chrome session.
        /// </summary>
        /// <param name="browser">Browser name. Possible values "chrome" (default), "firefox"</param>
        /// <returns>Unique session ID</returns>
        public static string CreateSession(string browser = "chrome")
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session", Method.POST) { RequestFormat = DataFormat.Json };
            var capabilityJson = new
            {
                desiredCapabilities = new
                {
                    browserName = browser,
                    platform = new
                    {
                        majorVersion = 0,
                        minorVersion = 0,
                        platformType = 0
                    },
                    version = "",
                    isJavaScriptEnabled = true
                }
            };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(capabilityJson), ParameterType.RequestBody);
            var response = client.Execute(request);

            var sessionID = "";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);

                if (responseObject["sessionId"] != null)
                {
                    sessionID = responseObject["sessionId"].ToString();
                }
            }
            return sessionID;
        }

        /// <summary>
        /// Quits the driver and deletes the session from Selenium Server
        /// </summary>
        /// <param name="sessionID">Session ID of the driver</param>
        public static void DeleteSession(string sessionID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}", Method.DELETE);
            request.AddUrlSegment("id", sessionID);
            client.Execute(request);
        }

        /// <summary>
        /// Navigate to a new URL
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="url">The URL to navigate to</param>
        public static void NavigateToUrl(string sessionID, string url)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/url", Method.POST) { RequestFormat = DataFormat.Json };
            request.AddUrlSegment("id", sessionID);
            var parameterJson = new { url };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(parameterJson), ParameterType.RequestBody);
            client.Execute(request);
        }

        /// <summary>
        /// Change focus to another frame on the page.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="frameHtmlID">Identifier for the frame to change focus to</param>
        public static void SwitchToFrame(string sessionID, string frameHtmlID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/frame", Method.POST) { RequestFormat = DataFormat.Json };
            request.AddUrlSegment("id", sessionID);
            var parameterJson = new { id = frameHtmlID };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(parameterJson), ParameterType.RequestBody);
            client.Execute(request);
        }

        /// <summary>
        /// Retrieve the list of all window handles available to the session, the first one is the main window handle.
        /// To get the popup window handle take the second element of the list.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <returns>List of window handles</returns>
        public static List<string> GetWindowHandles(string sessionID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/window_handles", Method.GET);
            request.AddUrlSegment("id", sessionID);
            var response = client.Execute(request);
            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            var responseArray = (JsonArray)responseObject["value"];

            return responseArray.Select(handleID => handleID != null ? handleID.ToString() : "").ToList();
        }

        /// <summary>
        /// Change focus to another window (popup) on the page for the specified session.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="windowHandleID">Identifier for the frame to change focus to</param>
        public static void SwitchToWindow(string sessionID, string windowHandleID = "")
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/window", Method.POST) { RequestFormat = DataFormat.Json };
            request.AddUrlSegment("id", sessionID);
            var parameterJson = new { name = windowHandleID };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(parameterJson), ParameterType.RequestBody);
            client.Execute(request);
        }

        /// <summary>
        /// Get the current page source.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <returns>The current page source.</returns>
        public static string GetPageSource(string sessionID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/source", Method.GET);
            request.AddUrlSegment("id", sessionID);
            var response = client.Execute(request);
            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            var text = responseObject["value"].ToString();
            return text;
        }

        /// <summary>
        /// Gets the text of the currently displayed JavaScript alert(), confirm(), or prompt() dialog.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <returns>The text of the currently displayed alert.</returns>
        public static string GetAlertText(string sessionID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/alert_text", Method.GET);
            request.AddUrlSegment("id", sessionID);
            var response = client.Execute(request);
            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            if (responseObject["status"].ToString().Equals("27"))
                return "-1";
            var text = responseObject["value"].ToString();
            return text;
        }

        /// <summary>
        /// Accepts the currently displayed alert dialog. Usually, this is equivalent to clicking on the 'OK' button in the dialog.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        public static void AcceptAlert(string sessionID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/accept_alert", Method.POST);
            request.AddUrlSegment("id", sessionID);
            client.Execute(request);
        }

        /// <summary>
        /// Dismisses the currently displayed alert dialog. For confirm() and prompt() dialogs, this is equivalent to clicking the 'Cancel' button. 
        /// For alert() dialogs, this is equivalent to clicking the 'OK' button.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        public static void DismissAlert(string sessionID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/dismiss_alert", Method.POST);
            request.AddUrlSegment("id", sessionID);
            client.Execute(request);
        }

        /// <summary>
        /// Inject a snippet of JavaScript into the page for execution in the context of the currently selected frame.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="script">The script to execute</param>
        /// <returns>The executed script is assumed to be synchronous and the result of evaluating the script is returned to the client.</returns>
        public static string ExecuteJavascript(string sessionID, string script)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/execute", Method.POST) { RequestFormat = DataFormat.Json };
            request.AddUrlSegment("id", sessionID);
            var parameterJson = new { script, args = new JsonArray() };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(parameterJson), ParameterType.RequestBody);
            var response = client.Execute(request);
            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            var text = responseObject["value"] != null ? responseObject["value"].ToString() : "";
            return text;
        }

        /// <summary>
        /// Search for an element on the page, starting from the document root.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="selectorValue">The search target</param>
        /// <param name="selectorType">Possible values: "id" (default), "name", "link text", "partial link text", "tag name", "class name", "css selector", "xpath"</param>
        /// <returns>Driver element ID (Returns -1 when element is not found.)</returns>
        public static int FindElement(string sessionID, string selectorValue, string selectorType = "id")
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/element", Method.POST) { RequestFormat = DataFormat.Json };
            request.AddUrlSegment("id", sessionID);
            var parameterJson = new { @using = selectorType, value = selectorValue };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(parameterJson), ParameterType.RequestBody);
            var response = client.Execute(request);
            
            var elementID = -1;
            if (response.StatusCode != HttpStatusCode.OK) 
                return elementID;

            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            var responseObjectValue = (JsonObject)responseObject["value"];

            if (responseObjectValue == null) 
                return elementID;

            if (!int.TryParse(responseObjectValue["ELEMENT"].ToString(), out elementID))
            {
                elementID = -1;
            }
            return elementID;
        }

        /// <summary>
        /// Returns the visible text for the html element
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="selectorValue">The search target</param>
        /// <param name="selectorType">Possible values: "id" (default), "name", "link text", "partial link text", "tag name", "class name", "css selector", "xpath"</param>
        /// <returns>Visible text for the html element</returns>
        public static string GetElementText(string sessionID, string selectorValue, string selectorType = "id")
        {
            var elementID = FindElement(sessionID, selectorValue, selectorType);

            return elementID == -1 ? "" : GetElementText(sessionID, elementID);
        }

        /// <summary>
        /// Returns the visible text for the driver element
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="driverElementID">Driver element ID (is found by FindElement method)</param>
        /// <returns>Visible text for the html element</returns>
        public static string GetElementText(string sessionID, int driverElementID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/element/{elementID}/text", Method.GET);
            request.AddUrlSegment("id", sessionID);
            request.AddUrlSegment("elementID", driverElementID.ToString("0"));
            var response = client.Execute(request);
            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            var text = responseObject["value"].ToString();
            return text;
        }

        /// <summary>
        /// Send a sequence of key strokes to a html element.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="elementText">Text to be written into the element (clears the content if not set)</param>
        /// <param name="selectorValue">The search target (selector value)</param>
        /// <param name="selectorType">Possible values: "id" (default), "name", "link text", "partial link text", "tag name", "class name", "css selector", "xpath"</param>
        public static void SetElementText(string sessionID, string elementText, string selectorValue, string selectorType = "id")
        {
            var elementID = FindElement(sessionID, selectorValue, selectorType);

            if (elementID == -1) 
                return;

            SetElementText(sessionID, elementText, elementID);
        }

        /// <summary>
        /// Send a sequence of key strokes to an element.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="elementText">Text to be written into the element (clears the content if not set)</param>
        /// <param name="driverElementID">Driver element ID (is found by FindElement method)</param>
        public static void SetElementText(string sessionID, string elementText, int driverElementID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/element/{elementID}/value", Method.POST);
            request.AddUrlSegment("id", sessionID);
            request.AddUrlSegment("elementID", driverElementID.ToString("0"));
            var parameterJson = new { value = new JsonArray { elementText } };
            request.AddParameter("application/json;charset=utf-8", SimpleJson.SerializeObject(parameterJson), ParameterType.RequestBody);
            client.Execute(request);
        }

        /// <summary>
        /// Select the specified option of the html select box.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="optionValue">The value of the selectbox option element to be selected</param>
        /// <param name="selectorValue">Selectbox name or id (differentiated by selectorType parameter)</param>
        /// <param name="selectorType">Possible values: "id" (default), "name"</param>
        public static void SetSelectOption(string sessionID, string optionValue, string selectorValue, string selectorType = "id")
        {
            var xpath = string.Format("//select[@{0}='{1}']/option[@value='{2}']", selectorType == "name" ? "name" : "id", selectorValue, optionValue);
            
            ClickOnElement(sessionID, xpath, "xpath");
        }

        /// <summary>
        /// Select the specified option of the html select box.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="optionText">The text of the selectbox option element to be selected</param>
        /// <param name="selectorValue">Selectbox name or id (differentiated by selectorType parameter)</param>
        /// <param name="selectorType">Possible values: "id" (default), "name"</param>
        public static void SetSelectOptionByOptionText(string sessionID, string optionText, string selectorValue, string selectorType = "id")
        {
            var xpath = string.Format("//select[@{0}='{1}']/option[normalize-space(text())='{2}']", selectorType == "name" ? "name" : "id", selectorValue, optionText);

            ClickOnElement(sessionID, xpath, "xpath");
        }

        /// <summary>
        /// Click on an element.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="selectorValue">The search target (selector value)</param>
        /// <param name="selectorType">Possible values: "id" (default), "name", "link text", "partial link text", "tag name", "class name", "css selector", "xpath"</param>
        public static void ClickOnElement(string sessionID, string selectorValue, string selectorType = "id")
        {
            var elementID = FindElement(sessionID, selectorValue, selectorType);

            if (elementID == -1)
                return;

            ClickOnElement(sessionID, elementID);
        }

        /// <summary>
        /// Click on an element.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="driverElementID">Driver element ID (is found by FindElement method)</param>
        public static void ClickOnElement(string sessionID, int driverElementID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/element/{elementID}/click", Method.POST);
            request.AddUrlSegment("id", sessionID);
            request.AddUrlSegment("elementID", driverElementID.ToString("0"));
            client.Execute(request);
        }

        /// <summary>
        /// Clear a textarea or text imput element's value.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="selectorValue">The search target (selector value)</param>
        /// <param name="selectorType">Possible values: "id" (default), "name", "link text", "partial link text", "tag name", "class name", "css selector", "xpath"</param>
        public static void ClearElementText(string sessionID, string selectorValue, string selectorType = "id")
        {
            var elementID = FindElement(sessionID, selectorValue, selectorType);

            if (elementID == -1)
                return;

            ClearElementText(sessionID, elementID);
        }

        /// <summary>
        /// Clear a textarea or text imput element's value.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="driverElementID">Driver element ID (is found by FindElement method)</param>
        public static void ClearElementText(string sessionID, int driverElementID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/element/{elementID}/clear", Method.POST);
            request.AddUrlSegment("id", sessionID);
            request.AddUrlSegment("elementID", driverElementID.ToString("0"));
            client.Execute(request);
        }

        /// <summary>
        /// Determine if an element is currently displayed.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="selectorValue">The search target (selector value)</param>
        /// <param name="selectorType">Possible values: "id" (default), "name", "link text", "partial link text", "tag name", "class name", "css selector", "xpath"</param>
        /// <returns>Whether the element is displayed.</returns>
        public static bool IsElementVisible(string sessionID, string selectorValue, string selectorType = "id")
        {
            var elementID = FindElement(sessionID, selectorValue, selectorType);
            
            return elementID != -1 && IsElementVisible(sessionID, elementID);
        }

        /// <summary>
        /// Determine if an element is currently displayed.
        /// </summary>
        /// <param name="sessionID">ID of the session to route the command to</param>
        /// <param name="driverElementID">Driver element ID (is found by FindElement method)</param>
        /// <returns>Whether the element is displayed.</returns>
        public static bool IsElementVisible(string sessionID, int driverElementID)
        {
            var client = new RestClient(ServerUrl);
            var request = new RestRequest("wd/hub/session/{id}/element/{elementID}/displayed", Method.GET);
            request.AddUrlSegment("id", sessionID);
            request.AddUrlSegment("elementID", driverElementID.ToString("0"));
            var response = client.Execute(request);
            var responseObject = SimpleJson.DeserializeObject<JsonObject>(response.Content);
            bool isVisible;
            bool.TryParse(responseObject["value"].ToString(), out isVisible);
            return isVisible;
        }


    }
}
