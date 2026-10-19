using System;
using System.Collections.Generic;
using System.Data;
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
    public class RoleRepository : IRoleRepository
    {

        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PagedResponse<IEnumerable<Role>>>> GetAllAsync(PaginationParams pagination)
        {
            var roles = new List<Role>();
            var response = new RepositoryResponse<PagedResponse<IEnumerable<Role>>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllRole", connection);
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
                            roles.Add(new Role
                            {
                                Id = (int)reader["Id"],
                                RoleName = reader["RoleName"].ToString()!,
                                State = reader["State"] != DBNull.Value ? (bool)reader["State"] : false
                            });
                        }

                        // segundo resultado: el total de registros para calcular páginas
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                            totalRecords = reader.GetInt32(0);
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = new PagedResponse<IEnumerable<Role>>
                    {
                        Data = roles,
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


        public async Task<RepositoryResponse<Role>> GetByIdAsync(int id)
        {
            var roleReturned = new Role();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetRoleById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            roleReturned.Id = (int)reader["Id"];
                            roleReturned.RoleName = (string)reader["RoleName"];
                            roleReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Role>
                    {
                        Data = roleReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }

        }

        public async Task<RepositoryResponse<Role>> GetByNameAsync(string name)
        {
            var role = new Role();
            var response = new RepositoryResponse<Role>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_GetRoleByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            role.Id = (int)reader["Id"];
                            role.RoleName = reader["RoleName"].ToString()!;
                            role.State = (bool)reader["State"];
                        }

                        else
                        {
                            role = new Role(); // Retornar un objeto rol vacío si no se encuentra ninguna coincidencia
                        }
                    }

                    //capturar codigo de retorno
                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    response.Data = role;
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
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }


        }

        public async Task<RepositoryResponse<Role>> AddAsync(Role role)
        {
            var roleReturned = new Role();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_InsertNewRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            roleReturned.Id = (int)reader["Id"];
                            roleReturned.RoleName = (string)reader["RoleName"];
                            roleReturned.State = (bool)reader["State"];
                        }
                    }

                    //capturar codigo de retorno
                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Role>
                    {
                        Data = roleReturned,
                        OperationStatusCode = retornedValue,
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }

            catch (Exception ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Role>> UpdateAsync(int id, Role role)
        {
            var roleUpdated = new Role();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                    cmd.Parameters.AddWithValue("@State", role.State);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            roleUpdated.Id = (int)reader["Id"];
                            roleUpdated.RoleName = (string)reader["RoleName"].ToString()!;
                            roleUpdated.State = (bool)reader["State"];
                        }
                    }

                    var retornedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Role>
                    {
                        Data = roleUpdated,
                        OperationStatusCode = retornedValue,
                    };

                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };

            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Role>> SetStateAsync(int roleId, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand cmd = new SqlCommand("USP_UpdateRoleState", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", roleId);
                    cmd.Parameters.AddWithValue("@State", state);

                    Role roleUpdated = null;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            roleUpdated = new Role
                            {
                                Id = (int)reader["Id"],
                                RoleName = reader["RoleName"].ToString(),
                                State = (bool)reader["State"]
                            };
                        }
                    }

                    return new RepositoryResponse<Role>
                    {
                        Data = roleUpdated,
                        OperationStatusCode = roleUpdated != null ? 0 : 1
                    };
                }
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Role>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }




    }
}






