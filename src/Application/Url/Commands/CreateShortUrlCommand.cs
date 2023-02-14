using System.Collections.Generic;
using System.Data;
using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.Data.SqlClient;
using UrlShortenerService.Application.Common.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
    }
}

public static class Helper
{
    public static string ToTinyUuid(this Guid guid)
    {
        return Convert.ToBase64String(guid.ToByteArray())[0..^2]  // remove trailing == padding 
            .Replace('+', '-')                          // escape (for filepath)
            .Replace('/', '_');                         // escape (for filepath)
    }
}



public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;     //wrap Connection String into context    
        _hashids = hashids;     //todo generate from alphabet....but how long?
    }


    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        string connectionstring = "Data Source = localhost\\SQLEXPRESS; Initial Catalog = testDatabase; Persist Security Info = True; User ID = adminUser; Password = passw0rd;Trust Server Certificate=true";
        string cuid = Helper.ToTinyUuid(Guid.NewGuid());
        SqlConnection con = new SqlConnection(connectionstring);
        try
        {
            con.Open();
            string sql = $"INSERT INTO dbo.Url (shortUrl, longUrl) VALUES (@shortUrl, @longUrl)";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@shortUrl", cuid);
                cmd.Parameters.AddWithValue("@longUrl", request.Url);
                cmd.CommandType = CommandType.Text;
                var t = cmd.ExecuteNonQuery();
                //TODO: if dataset exsistent: return it
                con.Close();
            }
            return (cuid);
        }
        catch (Exception ex)
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return (ex.Message.ToString());
        }
    }
}
