## **How the Program Works Together**

1. **Client sends a POST request** to `/api/invoices/generate` with the **invoice data in JSON** format.
2. The request data is **validated**. If invalid, an error message is returned.
3. If the request is valid:
   * The **HTML template** is loaded and populated with the invoice data.
   * The populated template is **converted to a PDF** using iText.
   * The **PDF is returned as a file** in the HTTP response.
