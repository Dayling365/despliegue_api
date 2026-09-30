using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

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
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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
                    SqlCommand cmd = new SqlCommand("USP_GetUnPrintedInvoices", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
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

                            //agg iteam invoice a la cola
                            _invoicesQueue.Enqueue(invoice);
                        }

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
                    OperationStatusCode= ex.Number
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
            var invoicePrinted = new Invoice();
            try
            {

                //verificar cola
                if (_invoicesQueue.TryPeek(out Invoice? ItemPeek))
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        SqlCommand cmd = new SqlCommand("USP_ToPrintInvoice", connection);
                        cmd.Parameters.AddWithValue("@Id", ItemPeek.Id);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                invoicePrinted = new Invoice();
                                invoicePrinted.Id = (int)reader["Id"];
                                invoicePrinted.CreatedDate = (DateTime)reader["CreatedDate"];
                                invoicePrinted. TotalAmount = (decimal)reader["TotalAmount"];
                                invoicePrinted.SaleId = (int)reader["SaleId"];
                                invoicePrinted. IsPrinted = (bool)reader["IsPrinted"];
                            }
                        }
                    }

                    return new RepositoryResponse<Invoice>
                    {
                        Data = invoicePrinted,
                        Message = "Factura para imprimir retornada exitosamente",
                        OperationStatusCode = 0
                    };


                }
               

                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    Message = "No hay facturas para imprimir",
                    OperationStatusCode = 1
                };
            }
            catch (SqlException sqlEx)
            {
                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    Message = sqlEx.Message
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
