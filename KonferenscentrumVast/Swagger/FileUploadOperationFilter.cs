using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace KonferenscentrumVast.Swagger
{
    /// <summary>
    /// Custom Swagger operation filter that enables proper display
    /// of file upload fields (IFormFile) in Swagger UI.
    /// </summary>
    /// <remarks>
    /// Swagger by default does not render IFormFile parameters correctly.
    /// This filter ensures that endpoints accepting FileUploadDto or IFormFile
    /// appear as multipart/form-data inputs with a file picker.
    /// </remarks>
    public class FileUploadOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Called for each API operation during Swagger generation.
        /// Detects file upload parameters and configures the request body schema accordingly.
        /// </summary>
        /// <param name="operation">The Swagger operation being processed.</param>
        /// <param name="context">Provides metadata about the API endpoint.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint has a file upload parameter (either IFormFile or FileUploadDto)
            var hasFileUpload = context.ApiDescription.ParameterDescriptions
                .Any(p => p.Type == typeof(Microsoft.AspNetCore.Http.IFormFile)
                       || p.Type.FullName == "KonferenscentrumVast.DTO.FileUploadDto");

            // If yes, modify the Swagger operation to use multipart/form-data
            if (hasFileUpload)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    // Defines a file input field in Swagger
                                    ["file"] = new OpenApiSchema { Type = "string", Format = "binary" },

                                    // Optional Booking and Facility IDs linked to the uploaded file
                                    ["bookingId"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true },
                                    ["facilityId"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true }
                                },
                                // The 'file' field is required in the upload request
                                Required = new HashSet<string> { "file" }
                            }
                        }
                    }
                };
            }
        }
    }
}
