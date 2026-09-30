using LibreriaDonCesar.Core.Common;
using LibreriaDonCesar.Core.Entities;
using LibreriaDonCesar.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDonCesar.DataAccess.Repositories
{
    public class InventoryMovementRepository : IInventoryMovementRepository
    {
        private readonly string _connectionString;

        public InventoryMovementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }


        public async Task<RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>> GetAllAsync(PaginationParams pagination)
        {
            var inventoriesMovement = new List<InventoryMovement>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<InventoryMovement>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllInventoryMovement", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageNumber", pagination.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pagination.PageSize);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int totalRecords = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // primer resultado: los registros de la página
                        while (await reader.ReadAsync())
                        {
                            inventoriesMovement.Add(new InventoryMovement
                            {
                                Id = (int)reader["Id"],
                                DateTime = (DateTime)reader["DateTime"],
                                ProductId = (int)reader["ProductId"],
                                MovementType = reader["MovementType"].ToString()!,
                                Quantity = (int)reader["Quantity"],
                                StockBefore = (int)reader["StockBefore"],
                                StockAfter = (int)reader["StockAfter"],
                                Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString()


                            });
                        }

                        // segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<InventoryMovement>>
                    {
                        Data = inventoriesMovement,
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

        public async Task<RepositoryResponse<InventoryMovement>> GetByIdAsync(int id)
        {
            var movementReturned = new InventoryMovement();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetInventoryMovementById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                           
                            movementReturned.Id = (int)reader["Id"];
                            movementReturned.DateTime = (DateTime)reader["DateTime"];
                            movementReturned.ProductId = (int)reader["ProductId"];
                            movementReturned.MovementType = reader["MovementType"].ToString()!;
                            movementReturned.Quantity = (int)reader["Quantity"];
                            movementReturned.StockBefore = (int)reader["StockBefore"];
                            movementReturned.StockAfter = (int)reader["StockAfter"];
                            movementReturned.Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString();
                        }

                    }
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<InventoryMovement>
                    {
                        Data = movementReturned,
                        OperationStatusCode = retornedValue,
                    };

                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<InventoryMovement>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<InventoryMovement>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<InventoryMovement>> AddAsync(InventoryMovement movement)
        {
            var movementReturned = new InventoryMovement();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_InsertInventoryMovement", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", movement.ProductId);
                    cmd.Parameters.AddWithValue("@MovementType", movement.MovementType);
                    cmd.Parameters.AddWithValue("@Quantity", movement.Quantity);
                    cmd.Parameters.AddWithValue("@Reason", (object?)movement.Reason ?? DBNull.Value);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            movementReturned.Id = (int)reader["Id"];
                            movementReturned.DateTime = (DateTime)reader["DateTime"];
                            movementReturned.ProductId = (int)reader["ProductId"];
                            movementReturned.MovementType = reader["MovementType"].ToString()!;
                            movementReturned.Quantity = (int)reader["Quantity"];
                            movementReturned.StockBefore = (int)reader["StockBefore"];
                            movementReturned.StockAfter = (int)reader["StockAfter"];
                            movementReturned.Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString();
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<InventoryMovement>
                    {
                        Data = movementReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<InventoryMovement>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<InventoryMovement>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<InventoryMovement>>> GetMovementsByProductIdAsync(int productId)
        {
          var movementsList = new List<InventoryMovement>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetMovementsByProductId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var movement = new InventoryMovement
                            {
                                Id = (int)reader["Id"],
                                DateTime = (DateTime)reader["DateTime"],
                                ProductId = (int)reader["ProductId"],
                                MovementType = reader["MovementType"].ToString()!,
                                Quantity = (int)reader["Quantity"],
                                StockBefore = (int)reader["StockBefore"],
                                StockAfter = (int)reader["StockAfter"],
                                Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString()
                            };
                            movementsList.Add(movement);
                        }
                    }
                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<IEnumerable<InventoryMovement>>
                    {
                        Data = movementsList,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<InventoryMovement>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<InventoryMovement>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
        public async Task<RepositoryResponse<IEnumerable<InventoryMovement>>> GetMovementsByTypeAsync(string movementType)
        {
            var movementsList = new List<InventoryMovement>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetMovementsByType", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MovementType", movementType);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var movement = new InventoryMovement
                            {
                                Id = (int)reader["Id"],
                                DateTime = (DateTime)reader["DateTime"],
                                ProductId = (int)reader["ProductId"],
                                MovementType = reader["MovementType"].ToString()!,
                                Quantity = (int)reader["Quantity"],
                                StockBefore = (int)reader["StockBefore"],
                                StockAfter = (int)reader["StockAfter"],
                                Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString()
                            };
                            movementsList.Add(movement);
                        }
                    }
                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<IEnumerable<InventoryMovement>>
                    {
                        Data = movementsList,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<InventoryMovement>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<InventoryMovement>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }

        }
        public async Task<RepositoryResponse<List<InventoryMovement>>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var movementsList = new List<InventoryMovement>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetMovementsByDateRange", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var movement = new InventoryMovement
                            {
                                Id = (int)reader["Id"],
                                DateTime = (DateTime)reader["DateTime"],
                                ProductId = (int)reader["ProductId"],
                                MovementType = reader["MovementType"].ToString()!,
                                Quantity = (int)reader["Quantity"],
                                StockBefore = (int)reader["StockBefore"],
                                StockAfter = (int)reader["StockAfter"],
                                Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString()
                            };
                            movementsList.Add(movement);
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<List<InventoryMovement>>
                    {
                        Data = movementsList,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<List<InventoryMovement>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<List<InventoryMovement>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
 }