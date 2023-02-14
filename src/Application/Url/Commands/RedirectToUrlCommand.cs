using FluentValidation;
using HashidsNet;
using MediatR;
using Microsoft.Data.SqlClient;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record RedirectToUrlCommand : IRequest<string>
{
    public string Id { get; init; } = default!;
}

public class RedirectToUrlCommandValidator : AbstractValidator<RedirectToUrlCommand>
{
    public RedirectToUrlCommandValidator()
    {
        _ = RuleFor(v => v.Id)
          .NotEmpty()
          .WithMessage("Id is required.");
    }
}

public class RedirectToUrlCommandHandler : IRequestHandler<RedirectToUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public RedirectToUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(RedirectToUrlCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        //TODO: into app settings
        string connectionstring = "Data Source = localhost\\SQLEXPRESS; Initial Catalog = testDatabase; Persist Security Info = True; User ID = adminUser; Password = passw0rd;Trust Server Certificate=true";
        try
        {
            //TODO: with Model first (Entity framework)
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                conn.Open();
                string commandtext = $"select * from dbo.Url where ShortUrl='{request.Id}'";
                SqlCommand cmd = new SqlCommand(commandtext, conn);
                var reader = cmd.ExecuteReader();
                reader.Read();
                var url = reader["LongUrl"].ToString() + "";
                return url;
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
