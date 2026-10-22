using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Data;

namespace LibreriaDonCesar.DataAccess.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        //Definir la instancia de la cola concurrente
        private readonly ConcurrentQueue<Invoice> _invoicesQueue = new ConcurrentQueue<Invoice>();
        //variable global para capturrtar la cadena de conexion
        private readonly string _connectionString;
        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<ConcurrentQueue<Invoice>>> GetInvoiceQueue()
        {
            try
            {
                //vaciar la cola
                _invoicesQueue.Clear();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // USP_GetUnPrintedInvoices es paginado: se recorren todas las páginas para llenar la cola completa
                    int pageNumber = 1;
                    int pageSize = 100;
                    int totalRecords = 0;
                    int totalPages = 1;

                    while (pageNumber <= totalPages)
                    {
                        SqlCommand cmd = new SqlCommand("USP_GetUnPrintedInvoices", connection);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var invoice = new Invoice();
                                //capturar los datos de la factura
                                invoice.Id = (int)reader["Id"];
                                invoice.CreatedDate = (DateTime)reader["CreatedDate"];
                                invoice.TotalAmount = (decimal)reader["TotalAmount"];
                                invoice.SaleId = (int)reader["SaleId"];
                                invoice.IsPrinted = (bool)reader["IsPrinted"];
                                invoice.PrintedDate = reader["PrintedDate"] != DBNull.Value ? (DateTime)reader["PrintedDate"] : null;

                                //agg iteam invoice a la cola
                                _invoicesQueue.Enqueue(invoice);
                            }

                            // segundo resultado: el total de registros para calcular las páginas
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                totalRecords = reader.GetInt32(0);
                        }

                        var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                        // 50037: no hay facturas pendientes de imprimir (la cola queda vacía)
                        if (returnedValue == 50037)
                            break;

                        if (returnedValue != 0)
                        {
                            return new RepositoryResponse<ConcurrentQueue<Invoice>>
                            {
                                Data = null,
                                OperationStatusCode = returnedValue
                            };
                        }

                        totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                        pageNumber++;
                    }
                }
                return new RepositoryResponse<ConcurrentQueue<Invoice>>
                {
                    Data = _invoicesQueue,
                    Message = "Factura Retornada Exitosamente",
                    OperationStatusCode = 0
                };
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<ConcurrentQueue<Invoice>>
                {
                    Data = null,
                    Message = ex.Message,
                    OperationStatusCode = ex.Number
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<ConcurrentQueue<Invoice>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Invoice>> ToPrint()
        {
            Invoice? invoicePrinted = null;
            try
            {
                //verificar cola
                if (_invoicesQueue.TryPeek(out Invoice? ItemPeek))
                {
                    int returnedValue;

                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        SqlCommand cmd = new SqlCommand("USP_ToPrintInvoice", connection);
                        cmd.Parameters.AddWithValue("@Id", ItemPeek.Id);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            // 1.er resultado: la factura
                            if (await reader.ReadAsync())
                            {
                                invoicePrinted = new Invoice();
                                invoicePrinted.Id = (int)reader["Id"];
                                invoicePrinted.CreatedDate = (DateTime)reader["CreatedDate"];
                                invoicePrinted.TotalAmount = (decimal)reader["TotalAmount"];
                                invoicePrinted.SaleId = (int)reader["SaleId"];
                                invoicePrinted.IsPrinted = (bool)reader["IsPrinted"];
                                invoicePrinted.PrintedDate = reader["PrintedDate"] != DBNull.Value ? (DateTime)reader["PrintedDate"] : null;
                            }

                            // 2.º resultado: las líneas de la venta
                            if (invoicePrinted != null && await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    invoicePrinted.Details.Add(new SaleDetail
                                    {
                                        Id = (int)reader["Id"],
                                        SaleId = (int)reader["SaleId"],
                                        ProductId = (int)reader["ProductId"],
                                        ProductName = reader["ProductName"].ToString(),
                                        ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                        PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                        Quantity = (int)reader["Quantity"],
                                        SalePrice = (decimal)reader["SalePrice"],
                                        LineTotal = (decimal)reader["LineTotal"]
                                    });
                                }
                            }
                        }

                        returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    }

                    return new RepositoryResponse<Invoice>
                    {
                        Data = invoicePrinted,
                        Message = "Factura para imprimir retornada exitosamente",
                        OperationStatusCode = returnedValue
                    };
                }

                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    Message = "No hay facturas para imprimir",
                    OperationStatusCode = 50037
                };
            }
            catch (SqlException sqlEx)
            {
                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    Message = sqlEx.Message,
                    OperationStatusCode = sqlEx.Number
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    Message = ex.Message,
                    OperationStatusCode = -1
                };
            }
        }
    }
}
