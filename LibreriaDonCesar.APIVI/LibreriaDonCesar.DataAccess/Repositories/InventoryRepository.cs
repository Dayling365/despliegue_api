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
        //Cmpo global para capturar la cadena de conexión a la base de datos    
        private readonly string _connectionString;

        //Constructor del repositorio
        public InventoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }


        //Metodo para obtener una lista de inventario, y retorna un objeto de tipo RepositoryResponse
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
                                ProductId= (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString()!,
                                SalePrice = (decimal)reader["SalePrice"],
                                UnitsInStock = (int)reader["UnitsInStock"],
                                PurchasePrice = (decimal)reader["PurchasePrice"]




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
            }
            return response;
        }
        public async Task<RepositoryResponse<Inventory>> GetByIdAsync(int id)
        {
            var inventoryReturned = new Inventory();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetInventoryByProductId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            inventoryReturned.Id = (int)reader["Id"];
                            inventoryReturned.ProductId = (int)reader["ProductId"];
                            inventoryReturned.ProductName = reader["ProductName"].ToString()!;
                            inventoryReturned.SalePrice = (decimal)reader["SalePrice"];
                            inventoryReturned.UnitsInStock = (int)reader["UnitsInStock"];
                            inventoryReturned.PurchasePrice = (decimal)reader["PurchasePrice"];
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Inventory>
                    {
                        Data = inventoryReturned,
                        OperationStatusCode = retornedValue,
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


        public async Task<RepositoryResponse<Inventory>> GetByNameAsync(string name)
        {
            var inventory = new Inventory();
            var response = new RepositoryResponse<Inventory>();
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
                        if (await reader.ReadAsync())
                        {
                            inventory.Id = (int)reader["Id"];
                            inventory.ProductId = (int)reader["ProductId"];
                            inventory.ProductName = reader["ProductName"].ToString()!;
                            inventory.SalePrice = (decimal)reader["SalePrice"];
                            inventory.UnitsInStock = (int)reader["UnitsInStock"];
                            inventory.PurchasePrice = (decimal)reader["PurchasePrice"];
                        }

                        else
                        {
                            inventory = new Inventory(); // Retornar un objeto Inventario vacío si no se encuentra ninguna coincidencia
                        }
                    }

                    //capturar codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = inventory;
                    response.OperationStatusCode = returnedValue;

                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;

                return response;

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

    }
}
