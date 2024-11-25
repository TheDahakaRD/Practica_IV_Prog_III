using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;    

namespace Consoleprueba
{
    class Program
    {
            static void Main(string[] args)
        {
            // Configuración del reporte
            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "TestReport.html");
            var htmlReporter = new ExtentSparkReporter(reportPath);
            var extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
            

            // Crear casos de prueba en el reporte
            var testSuccess = extent.CreateTest("Prueba de Login - Caso de Éxito");
            var testFailure = extent.CreateTest("Prueba de Login - Caso de Fracaso");

            IWebDriver driver = null!;

            try
            {
                // Configuración del Edge WebDriver
                var options = new EdgeOptions();
                driver = new EdgeDriver(options);

                #region Caso de Éxito
                // Prueba de inicio de sesión exitoso
                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Navegando a la página de inicio de sesión");
                driver.Navigate().GoToUrl("https://github.com/login");
                driver.Manage().Window.Maximize();

                // Capturar pantalla inicial
                string screenshotPath = CaptureScreenshot(driver, "LoginPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página de inicio de sesión");

                // Automatizar el formulario de inicio de sesión con credenciales correctas
                driver.FindElement(By.Id("login_field")).SendKeys("Usuario");//coloque su usuario
                driver.FindElement(By.Id("password")).SendKeys("Contraseña");//Coloque su contraseña
                driver.FindElement(By.Name("commit")).Click();

                // Capturar pantalla después de iniciar sesión
                screenshotPath = CaptureScreenshot(driver, "LoggedInPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página después del inicio de sesión");

                // Esperar a que la página cargue
                System.Threading.Thread.Sleep(2000);

                // Hacer clic en el repositorio 
                var repoLink = driver.FindElement(By.CssSelector("a[href='/**']")); //Remplace las comillas por el nombre de usuario y repositoro
                repoLink.Click();

                // Esperar que se cargue el repositorio
                System.Threading.Thread.Sleep(2000);

                screenshotPath = CaptureScreenshot(driver, "LoggedInRepoPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página del Repositorio");

                // Hacer clic en el archivo 
                driver.FindElement(By.LinkText("**")).Click(); //Remlace las comillas por el nombre del archivo 

                // Esperar a que se cargue el PDF
                System.Threading.Thread.Sleep(3000);

                // Hacer scroll hacia abajo dentro del archivo
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollBy(0, 1000);");

                // Capturar pantalla del archivo abierrto
                screenshotPath = CaptureScreenshot(driver, "Archivo");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Archivo abierto");

                testSuccess.Pass("Inicio de sesión exitoso, repositorio abierto y archivo 1 cargado.");

                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Cerrando sesión");
                driver.Navigate().GoToUrl("https://github.com/logout");
                System.Threading.Thread.Sleep(2000);
                                screenshotPath = CaptureScreenshot(driver, "SingOut");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Cerrando cession");
                driver.FindElement(By.Name("commit")).Click();
                System.Threading.Thread.Sleep(2000);

                #endregion

                #region Caso de Fracaso
                // Prueba de inicio de sesión fallido con credenciales incorrectas
                testFailure.Log(AventStack.ExtentReports.Status.Info, "Navegando a la página de inicio de sesión");
                driver.Navigate().GoToUrl("https://github.com/login");
                driver.Manage().Window.Maximize();

                // Capturar pantalla inicial
                screenshotPath = CaptureScreenshot(driver, "LoginPage_Failure");
                testFailure.AddScreenCaptureFromPath(screenshotPath, "Página de inicio de sesión");

                // Automatizar el formulario de inicio de sesión con credenciales incorrectas
                driver.FindElement(By.Id("login_field")).SendKeys("UsuarioInexistente");
                driver.FindElement(By.Id("password")).SendKeys("ClaveIncorrecta");
                driver.FindElement(By.Name("commit")).Click();

                // Capturar pantalla después del intento de inicio de sesión
                screenshotPath = CaptureScreenshot(driver, "FailedLoginAttempt");
                testFailure.AddScreenCaptureFromPath(screenshotPath, "Página después del intento de inicio de sesión");

                // Verificar mensaje de error
                try
                {
                    var errorMessage = driver.FindElement(By.CssSelector(".flash-error")).Text;
                    if (errorMessage.Contains("Incorrect username or password."))
                    {
                        testFailure.Pass("Inicio de sesión fallido como se esperaba: " + errorMessage);
                    }
                    else
                    {
                        testFailure.Fail("El mensaje de error no es el esperado.");
                    }
                }
                catch (NoSuchElementException)
                {
                    testFailure.Fail("No se encontró el mensaje de error esperado.");
                }
                #endregion
            }
            catch (Exception ex)
            {
                // Si ocurre un error general
                testFailure.Fail("Error durante la prueba: " + ex.Message);
            }
            finally
            {
                // Cerrar el navegador
                driver?.Quit();
                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Navegador cerrado");
                testFailure.Log(AventStack.ExtentReports.Status.Info, "Navegador cerrado");

                // Generar el reporte
                extent.Flush();
            }
        }

        // Método para capturar capturas de pantalla
        static string CaptureScreenshot(IWebDriver driver, string screenshotName)
        {
            try
            {
                string screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                string screenshotPath = Path.Combine(screenshotsDir, $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);

                return screenshotPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al capturar la pantalla: " + ex.Message);
                return null!;
            }
        }
    }
}
