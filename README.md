InteractiveWebDriver
====================

Selenium web driver implementation based on json wire protocol

I am creating a new selenium web driver implementation to provide an interactive session based web driver usages.

Normally, you can do this by storing webdrive instance in session but I don't like the session based solutions and therefore I decided to implement the methods found in RemoteWebDriver or ChromeDriver with an additional sessionID parameter. 

