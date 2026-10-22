using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LibreriaDonCesar.DataAccess.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly string _connectionString;

        //Constructor del repositorio
        public PurchaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetAllAsync(PaginationParams pagination)
        {
            var purchases = new List<PurchaseTransaction>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllPurchases", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 1. Primer resultado: las cabeceras de la página (el detalle se consulta aparte por Id)
                        while (await reader.ReadAsync())
                        {
                            purchases.Add(new PurchaseTransaction
                            {
                                Master = new Purchase
                                {
                                    Id = (int)reader["Id"],
                                    PurchaseDate = (DateTime)reader["PurchaseDate"],
                                    SupplierId = (int)reader["SupplierId"],
                                    SupplierName = reader["SupplierName"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<PurchaseDetail>()
                            });
                        }

                        // 2. Segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalRecords = reader.GetInt32(0);
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<PurchaseTransaction>>
                    {
                        Data = purchases,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalRecords = totalRecords
                    };
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<PurchaseTransaction>> GetByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetPurchasesById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", id);

                    var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    PurchaseTransaction? transaction = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            transaction = new PurchaseTransaction
                            {
                                Master = new Purchase
                                {
                                    Id = (int)reader["Id"],
                                    PurchaseDate = (DateTime)reader["PurchaseDate"],
                                    SupplierId = (int)reader["SupplierId"],
                                    SupplierName = reader["SupplierName"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<PurchaseDetail>()
                            };
                        }

                        if (transaction != null)
                        {
                            await reader.NextResultAsync();
                            var detailsList = new List<PurchaseDetail>();
                            while (await reader.ReadAsync())
                            {
                                detailsList.Add(new PurchaseDetail
                                {
                                    Id = (int)reader["Id"],
                                    PurchaseId = (int)reader["PurchaseId"],
                                    ProductId = (int)reader["ProductId"],
                                    ProductName = reader["ProductName"].ToString(),
                                    ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                    PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                    Quantity = (int)reader["Quantity"],
                                    PurchasePrice = (decimal)reader["PurchasePrice"],
                                    LineTotal = (decimal)reader["LineTotal"]
                                });
                            }
                            transaction.Details = detailsList;
                        }
                    }

                    var returnedValue = Convert.ToInt32(returnParam.Value);

                    return new RepositoryResponse<PurchaseTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<PurchaseDetail>>> GetDetailByIdAsync(int purchaseId)
        {
            var detailReturned = new List<PurchaseDetail>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetDetailsByPurchaseId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var detail = new PurchaseDetail
                            {
                                Id = (int)reader["Id"],
                                PurchaseId = (int)reader["PurchaseId"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                Quantity = (int)reader["Quantity"],
                                PurchasePrice = (decimal)reader["PurchasePrice"],
                                LineTotal = (decimal)reader["LineTotal"]
                            };
                            detailReturned.Add(detail);
                        }
                    }

                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<IEnumerable<PurchaseDetail>>
                    {
                        Data = detailReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<PurchaseDetail>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<PurchaseDetail>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<PurchaseTransaction>> InsertAsync(Purchase master, IEnumerable<PurchaseDetail> details)
        {
            PurchaseTransaction? transaction = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_InsertPurchase", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SupplierId", master.SupplierId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", master.PurchaseDate);

                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));
                    detailsTable.Columns.Add("PurchasePrice", typeof(decimal));

                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(item.ProductId, item.Quantity, item.PurchasePrice);
                    }

                    SqlParameter detailsParam = cmd.Parameters.AddWithValue("@PurchaseDetails", detailsTable);
                    detailsParam.SqlDbType = SqlDbType.Structured;
                    detailsParam.TypeName = "dbo.PurchaseDetailsType";

                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 1) la compra
                        if (await reader.ReadAsync())
                        {
                            transaction = new PurchaseTransaction();
                            transaction.Master = new Purchase
                            {
                                Id = (int)reader["Id"],
                                PurchaseDate = (DateTime)reader["PurchaseDate"],
                                SupplierId = (int)reader["SupplierId"],
                                SupplierName = reader["SupplierName"].ToString(),
                                TotalAmount = (decimal)reader["TotalAmount"]
                            };
                        }

                        if (transaction != null)
                        {
                            // 2) el detalle
                            var detailsList = new List<PurchaseDetail>();

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    detailsList.Add(new PurchaseDetail
                                    {
                                        Id = (int)reader["Id"],
                                        PurchaseId = (int)reader["PurchaseId"],
                                        ProductId = (int)reader["ProductId"],
                                        ProductName = reader["ProductName"].ToString(),
                                        ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                        PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                        Quantity = (int)reader["Quantity"],
                                        PurchasePrice = (decimal)reader["PurchasePrice"],
                                        LineTotal = (decimal)reader["LineTotal"]
                                    });
                                }
                            }
                            transaction.Details = detailsList;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<PurchaseTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination)
        {
            var purchases = new List<PurchaseTransaction>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_PurchasesByDateRange", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            purchases.Add(new PurchaseTransaction
                            {
                                Master = new Purchase
                                {
                                    Id = (int)reader["Id"],
                                    PurchaseDate = (DateTime)reader["PurchaseDate"],
                                    SupplierId = (int)reader["SupplierId"],
                                    SupplierName = reader["SupplierName"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<PurchaseDetail>()
                            });
                        }

                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalRecords = reader.GetInt32(0);
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<PurchaseTransaction>>
                    {
                        Data = purchases,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalRecords = totalRecords
                    };
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PagedResponse<IEnumerable<PurchaseTransaction>>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
