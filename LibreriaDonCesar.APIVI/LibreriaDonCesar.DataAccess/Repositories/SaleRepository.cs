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
    public class SaleRepository : ISaleRepository
    {
        private readonly string _connectionString;

        //Constructor del repositorio
        public SaleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>> GetAllAsync(PaginationParams pagination)
        {
            var sales = new List<SaleTransaction>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<SaleTransaction>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllSale", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // 1. Primer resultado: las cabeceras de la página
                        while (await reader.ReadAsync())
                        {
                            sales.Add(new SaleTransaction
                            {
                                Master = new Sale
                                {
                                    Id = (int)reader["Id"],
                                    SaleDate = (DateTime)reader["SaleDate"],
                                    CustomerType = reader["CustomerType"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<SaleDetail>()
                            });
                        }

                        // 2. Saltar al Segundo resultado: los detalles de esas compras
                        await reader.NextResultAsync();

                        while (await reader.ReadAsync())
                        {
                            var detail = new SaleDetail
                            {
                                Id = (int)reader["Id"],
                                SaleId = (int)reader["SaleId"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                SalePrice = (decimal)reader["SalePrice"],
                                LineTotal = (decimal)reader["LineTotal"],
                                 Quantity = (int)reader["Quantity"],
                            };

                            var transaction = sales.FirstOrDefault(s => s.Master.Id == detail.SaleId);
                            if (transaction != null)
                            {
                                transaction.Details = transaction.Details.Concat(new[] { detail }).ToList();
                            }
                        }

                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalRecords = reader.GetInt32(0);
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<SaleTransaction>>
                    {
                        Data = sales,
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
            }
            return response;
        }
        public async Task<RepositoryResponse<SaleTransaction>> GetByIdAsync(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetSaleById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", id);

                    var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    SaleTransaction transaction = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            transaction = new SaleTransaction
                            {
                                Master = new Sale
                                {
                                    Id = (int)reader["Id"],
                                  
                                    SaleDate = (DateTime)reader["SaleDate"],
                                    CustomerType = reader["CustomerType"].ToString(),
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<SaleDetail>()
                            };
                        }

                        if (transaction != null)
                        {
                            await reader.NextResultAsync();
                            var detailsList = new List<SaleDetail>();
                            while (await reader.ReadAsync())
                            {
                                detailsList.Add(new SaleDetail
                                {
                                    Id = (int)reader["Id"],
                                    SaleId = (int)reader["SaleId"],
                                    ProductId = (int)reader["ProductId"],
                                    ProductName = reader["ProductName"].ToString(),
                                    Quantity = (int)reader["Quantity"],
                                    SalePrice = (decimal)reader["SalePrice"],
                                    LineTotal = (decimal)reader["LineTotal"]
                                });
                            }
                            transaction.Details = detailsList;
                        }
                    }

                    var returnedValue = Convert.ToInt32(returnParam.Value);

                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = transaction, 
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


        public async Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetail> details)
        {
            var transaction = new SaleTransaction();

            try
            {

              

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                 

                    SqlCommand cmd = new SqlCommand("USP_InsertSale", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerType", master.CustomerType);                   
                    cmd.Parameters.AddWithValue("@SaleDate", master.SaleDate);


                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));


                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(item.ProductId, item.Quantity);
                    }

                    SqlParameter detailsParam = cmd.Parameters.AddWithValue("@SaleDetails", detailsTable);
                    detailsParam.SqlDbType = SqlDbType.Structured;
                    detailsParam.TypeName = "SaleDetailType";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            transaction.Master = new Sale
                            {
                                Id = (int)reader["Id"],
                                CustomerType = reader["CustomerType"].ToString(),
                                SaleDate = (DateTime)reader["SaleDate"],
                                TotalAmount = (decimal)reader["TotalAmount"]

                            };


                        }

                        await reader.NextResultAsync();

                        var detailsList = new List<SaleDetail>();

                        while (await reader.ReadAsync())
                        {
                            detailsList.Add(new SaleDetail
                            {
                                Id = (int)reader["Id"],
                                SaleId = (int)reader["SaleId"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = (string)reader["ProductName"],
                                Quantity = (int)reader["Quantity"],
                                SalePrice = (decimal)reader["SalePrice"],
                                LineTotal = (decimal)reader["LineTotal"]
                            });
                        }
                        transaction.Details = detailsList;

                    }
                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = 0,
                        Message = "Operacion exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }


        }

        public async Task<RepositoryResponse<IEnumerable<SaleDetail>>> GetDetailByIdAsync(int saleId)
        {
            var detailReturned = new List<SaleDetail>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetDetailsBySaleId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SaleId", saleId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var detail = new SaleDetail
                            {
                                Id = (int)reader["Id"],
                               SaleId = (int)reader["SaleId"],
                                ProductId = (int)reader["ProductId"],
                                ProductName= (string)reader["ProductName"],
                                Quantity = (int)reader["Quantity"],
                                SalePrice = (decimal)reader["SalePrice"],
                                LineTotal = (decimal)reader["LineTotal"]
                            };
                            detailReturned.Add(detail);
                        }
                    }

                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<IEnumerable<SaleDetail>>
                    {
                        Data = detailReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<SaleDetail>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<SaleDetail>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<List<SaleTransaction>>> GetSaleByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = new List<SaleTransaction>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_SaleByDateRange", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {

                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new SaleTransaction
                            {
                                Master = new Sale
                                {
                                    Id = (int)reader["Id"],
                                    CustomerType = reader["CustomerType"].ToString(),
                                    SaleDate = (DateTime)reader["SaleDate"],
                                    TotalAmount = (decimal)reader["TotalAmount"]
                                },
                                Details = new List<SaleDetail>()
                            });
                        }

                        await reader.NextResultAsync();

                        while (await reader.ReadAsync())
                        {
                            var detail = new SaleDetail
                            {
                                SaleId = (int)reader["SaleId"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                SalePrice = (decimal)reader["SalePrice"],
                                LineTotal = (decimal)reader["LineTotal"]
                            };

                            var transaction = transactions.FirstOrDefault(t => t.Master.Id == detail.SaleId);
                            if (transaction != null)
                            {
                                ((List<SaleDetail>)transaction.Details).Add(detail);
                            }
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<List<SaleTransaction>>
                    {
                        Data = transactions,
                        OperationStatusCode = returnedValue,
                        Message = transactions.Any() ? "Operación exitosa" : "No se encontraron compras en el rango de fechas"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<List<SaleTransaction>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<List<SaleTransaction>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


    }
}


    

