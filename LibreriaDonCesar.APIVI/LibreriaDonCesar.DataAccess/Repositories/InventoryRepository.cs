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
    public class InventoryRepository : IInventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>> GetAllAsync(PaginationParams pagination)
        {
            var inventories = new List<Inventory>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllInventory", connection);
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
                            inventories.Add(new Inventory
                            {
                                Id = (int)reader["Id"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                UnitsInStock = (int)reader["UnitsInStock"],
                                MinStock = (int)reader["MinStock"],
                                IsLowStock = (bool)reader["IsLowStock"],
                                StockStatus = reader["StockStatus"].ToString()
                            });
                        }

                        // segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<Inventory>>
                    {
                        Data = inventories,
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

        public async Task<RepositoryResponse<Inventory>> GetByProductIdAsync(int productId)
        {
            Inventory? inventoryReturned = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_GetInventoryByProductId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", productId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            inventoryReturned = new Inventory();
                            inventoryReturned.Id = (int)reader["Id"];
                            inventoryReturned.ProductId = (int)reader["ProductId"];
                            inventoryReturned.ProductName = reader["ProductName"].ToString();
                            inventoryReturned.ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null;
                            inventoryReturned.PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null;
                            inventoryReturned.SalePrice = Convert.ToDecimal(reader["SalePrice"]);
                            inventoryReturned.UnitsInStock = (int)reader["UnitsInStock"];
                            inventoryReturned.MinStock = (int)reader["MinStock"];
                            inventoryReturned.IsLowStock = (bool)reader["IsLowStock"];
                            inventoryReturned.StockStatus = reader["StockStatus"].ToString();
                        }
                    }

                    //capturamos el codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Inventory>
                    {
                        Data = inventoryReturned,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        // Devuelve el inventario de TODAS las variantes del producto con ese nombre
        public async Task<RepositoryResponse<IEnumerable<Inventory>>> GetByNameAsync(string name)
        {
            var inventories = new List<Inventory>();
            var response = new RepositoryResponse<IEnumerable<Inventory>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetInventoryByProductName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            inventories.Add(new Inventory
                            {
                                Id = (int)reader["Id"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                UnitsInStock = (int)reader["UnitsInStock"],
                                MinStock = (int)reader["MinStock"],
                                IsLowStock = (bool)reader["IsLowStock"],
                                StockStatus = reader["StockStatus"].ToString()
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = inventories;
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
                return response;
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<Inventory>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        // Cambia el umbral mínimo de stock de un producto
        public async Task<RepositoryResponse<Inventory>> UpdateMinStockAsync(int productId, int minStock)
        {
            Inventory? inventoryUpdated = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateMinStock", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@MinStock", minStock);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            inventoryUpdated = new Inventory
                            {
                                Id = (int)reader["Id"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                UnitsInStock = (int)reader["UnitsInStock"],
                                MinStock = (int)reader["MinStock"],
                                IsLowStock = (bool)reader["IsLowStock"],
                                StockStatus = reader["StockStatus"].ToString()
                            };
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Inventory>
                    {
                        Data = inventoryUpdated,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        // Productos que están en el umbral mínimo o por debajo (los más críticos primero)
        public async Task<RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>> GetLowStockAsync(PaginationParams pagination)
        {
            var inventories = new List<Inventory>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<Inventory>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetLowStockAlerts", connection);
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
                            inventories.Add(new Inventory
                            {
                                Id = (int)reader["Id"],
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"].ToString() : null,
                                PresentationName = reader["PresentationName"] != DBNull.Value ? reader["PresentationName"].ToString() : null,
                                UnitsInStock = (int)reader["UnitsInStock"],
                                MinStock = (int)reader["MinStock"],
                                UnitsMissing = (int)reader["UnitsMissing"],
                                StockStatus = reader["StockStatus"].ToString(),
                                IsLowStock = true // este SP solo devuelve productos con stock bajo
                            });
                        }

                        // segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<Inventory>>
                    {
                        Data = inventories,
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
    }
}
