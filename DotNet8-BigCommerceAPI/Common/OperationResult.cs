using BeachCommerce.Models;

namespace BeachCommerce.Common
{
    public class OperationResult<T> // Usar en POST, PUT, PATCH, DELETE
    {
        // Usado para indicar si la operación fue exitosa o no (ejemplo, despues de subir una imagen)
        public bool Success { get; set; }
        // Mensaje de error o de éxito que debe llegar directamente de la API de BigCommerce
        public string Message { get; set; }
        // Código de estado HTTP de la respuesta
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public Error Errors { get; set; }
    }
}
