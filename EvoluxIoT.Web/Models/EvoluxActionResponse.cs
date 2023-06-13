using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EvoluxIoT.Web.Models
{
    static public class EvoluxActionResponseMessages
    {
        public const string NoMessage = "";
        public const string DatabaseContextNull = "[ERROR] The database context is not correctly initialized. This can be happening due to connection errors or configuration issues in the database server ";
        public const string DatabaseContextEntityNull = "[ERROR] The database context for a givem entity is not correctly initialized. This can be happening due to a outdated or incomplete/broken database migration or a broken database table/scheme in our database server";
        public const string EntityNotFound = "[ERROR] The entity was not found in the database. This can be happening due to the action that is currently being evoked requires the specified entity to exist in the database such as editing or deleting it";
        public const string EntityFound = "[ERROR] The entity was found in the database. This can be happening due to the action that is currently being evoked requires the specified entity to not exist in the database such as creating it";
    }

    public static class EvoluxActionResponseExtensions
    {
        /// <summary>
        /// Sets the response as a successful response with the provided data.
        /// </summary>
        /// <param name="response">The EvoluxActionResponse instance.</param>
        /// <param name="data">The data to be returned.</param>
        public static void SetSuccess(this EvoluxActionResponse response, object data)
        {
            response.Success = true;
            response.Message = EvoluxActionResponseMessages.NoMessage;
            response.Data = data;
        }

        /// <summary>
        /// Sets the response as a failed response with the specified error message.
        /// </summary>
        /// <param name="response">The EvoluxActionResponse instance.</param>
        /// <param name="errorMessage">The error message to be displayed.</param>
        public static void SetFailure(this EvoluxActionResponse response, string errorMessage)
        {
            response.Success = false;
            response.Message = errorMessage;
            response.Data = null;
        }

        /// <summary>
        /// Sets the response as a failed response with the specified exception.
        /// </summary>
        /// <param name="response">The EvoluxActionResponse instance.</param>
        /// <param name="exception">The exception that occurred.</param>
        public static void SetFailure(this EvoluxActionResponse response, Exception exception)
        {
            response.Success = false;
            response.Message = exception.Message;
            response.Data = null;
            response.Exception = exception;
        }

        /// <summary>
        /// Sets the response as a failed response indicating that the database context is null.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="exception"></param>
        public static void SetDatabaseContextFailure(this EvoluxActionResponse response, Exception? exception = null)
        {
            response.Success = false;
            response.Message = EvoluxActionResponseMessages.DatabaseContextNull;
            response.Data = null;
            response.Exception = exception;
        }

        /// <summary>
        /// Sets the response as a failed response indicating that the database context entity is null.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="exception"></param>
        public static void SetDatabaseContextEntityFailure(this EvoluxActionResponse response, Exception? exception = null)
        {
            response.Success = false;
            response.Message = EvoluxActionResponseMessages.DatabaseContextEntityNull;
            response.Data = null;
            response.Exception = exception;
        }

        /// <summary>
        /// Sets the response as a failed response indicating that the entity cannot be found.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="exception"></param>
        public static void SetEntityNotFoundFailure(this EvoluxActionResponse response, Exception? exception = null)
        {
            response.Success = false;
            response.Message = EvoluxActionResponseMessages.EntityNotFound;
            response.Data = null;
            response.Exception = exception;
        }
    }


    /// <summary>
    /// Class to be used as a response to an action executed in the application.
    /// </summary>
    public class EvoluxActionResponse
    {
        /// <summary>
        /// Internal flag to indicate success or failure in the action executed.
        /// </summary>
        private bool _success = false;

        /// <summary>
        /// Internal flag to store the exception thrown in the action executed if any.
        /// </summary>
        private Exception? _exception = null;

        /// <summary>
        /// Success or failure in the action executed.
        /// </summary>
        public bool Success
        {
            get => _success && _exception is null;
            set => _success = value;
        }

        /// <summary>
        /// Exception thrown in the action executed if any.
        /// </summary>
        public Exception? Exception
        {
            get => _exception;
            set => _exception = value;
        }

        /// <summary>
        /// Friendly message to be displayed to the user.
        /// </summary>
        public string Message { get; set; } = EvoluxActionResponseMessages.NoMessage;

        /// <summary>
        /// Data to be returned to the user.
        /// </summary>
        public object? Data { get; set; } = null;

        /// <summary>
        /// Builds a IActionResult json instance from the current EvoluxActionResponse instance, allowing the user to specify the http status code.
        /// </summary>
        public IActionResult ToJsonResult(HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return new JsonResult(this)
            {
                StatusCode = (int)httpStatusCode
            };
        }


    }
}
