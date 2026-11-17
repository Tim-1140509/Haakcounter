using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

List<Project> projecten = new();

string RenderPage()
{
    var actiefProject = projecten.FirstOrDefault(p => p.Actief);

    int huidigeTelling = actiefProject?.HuidigeTelling ?? 0;
    int doelSteken = actiefProject?.Doelsteken ?? 0;

    string projectNaam = actiefProject == null
        ? "Selecteer een project"
        : System.Net.WebUtility.HtmlEncode(actiefProject.Naam);

    string status = actiefProject == null
        ? "Kies een project om te beginnen"
        : actiefProject.Afgerond
            ?"Project afgerond!"
            : $"Nog {doelSteken - huidigeTelling} steken te gaan";
    
    string html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Haak Counter</title>
        <link rel='stylesheet' href='/style.css'>
    </head>

    <body>
        <div class='container'>

            <h1>Hey Naema, hier is jouw persoonlijke Haak Counter.</h1>
            <p class='subtitle'>Houd je haakwerkje eenvoudig bij.</p>

            
                <div class='counter-card'>
                    <p class='counter-label'>{projectNaam}</p>
                    <h2 id='counter'>{huidigeTelling}</h2>
                    <p id='counter-status'>{status}</p>
                    <br>

                    <div class='buttons'>
                        <form method='post' action='/minus'>
                            <button type='submit' class='minus'>-</button>
                        </form>

                        <form method='post' action='/plus'>
                            <button type='submit' class='plus'>+</button>
                        </form>
                    </div>
                </div>

                <div class='info-card'>
                    <h3>Mijn Projecten</h3>
                    <ul>
                        {string.Join("", projecten.Select(p =>
                        $@"<li class='project-item'>
                            <div class='project-info'>
                                <strong>{System.Net.WebUtility.HtmlEncode(p.Naam)}</strong>
                                <br>
                                <span id='project-count-{p.Id}' data-target='{p.Doelsteken}'>{p.HuidigeTelling}</span>
                                <span id='project-status-{p.Id}'>
                                    {(p.Afgerond ? "<span class='project-klaar'>Klaar!</span>" : "")}
                                </span>
                            </div>

                            <div class='project-actions'>
                                <form method='post' action='/activeren/{p.Id}'>
                                    <button type='submit'
                                        class='project-switch {(p.Actief ? "active" : "")}'>
                                        <span></span>
                                    </button>
                                </form>

                                <form method='post' action='/verwijderen/{p.Id}'>
                                    <button type='submit' class='delete-button'>Verwijderen</button>
                                </form>
                            </div>
                        </li>"
                        ))}
                     </ul>
                </div>

                <form method='post' action='/reset'>
                    <button type='submit' class='reset'>Reset teller</button>
                </form>
                <br>
                <a href='/toevoegen' class='Iets-toevoegen'>Iets nieuws toevoegen</a>
            

        </div>

<script>

function startConfetti() {{
    const canvas = document.createElement('canvas');
    canvas.id = 'confetti-canvas';
    canvas.style.position = 'fixed';
    canvas.style.top = '0';
    canvas.style.left = '0';
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    canvas.style.pointerEvents = 'none';
    canvas.style.zIndex = '9999';

    document.body.appendChild(canvas);

    const ctx = canvas.getContext('2d');

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    const colors = ['#55476a', '#ae3d63', '#db3853', '#f45c44', '#f8b646'];
    const particles = [];

    for (let i = 0; i < 250; i++) {{
        particles.push({{
            x: Math.random() * canvas.width,
            y: Math.random() * -canvas.height,
            size: Math.random() * 6 + 2,
            speed: Math.random() * 4 + 2,
            color: colors[Math.floor(Math.random() * colors.length)],
            rotation: Math.random() * 360
        }});
    }}

    let animationId;

    function animateConfetti() {{
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        particles.forEach(p => {{
            ctx.fillStyle = p.color;
            ctx.save();
            ctx.translate(p.x, p.y);
            ctx.rotate(p.rotation * Math.PI / 180);
            ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size);
            ctx.restore();

            p.y += p.speed;
            p.rotation += 3;

            if (p.y > canvas.height) {{
                p.y = -10;
                p.x = Math.random() * canvas.width;
            }}
        }});

        animationId = requestAnimationFrame(animateConfetti);
    }}

    animateConfetti();

    setTimeout(() => {{
        cancelAnimationFrame(animationId);
        canvas.remove();
    }}, 5000);
}}

    document.querySelectorAll('.buttons form').forEach(form => {{
        form.addEventListener('submit', async function(event) {{
            event.preventDefault();

            const button = form.querySelector('button');
            button.disabled = true;

            try {{
                const response = await fetch(form.action, {{
                    method: 'POST'
                }});

                if (!response.ok) {{
                    throw new Error('Tellen is mislukt');
                }}

                const data = await response.json();

                document.getElementById('counter').textContent = data.count;
                document.getElementById('counter-status').textContent = data.status;

                if (data.projectId) {{
                    const projectCount = document.getElementById(
                        'project-count-' + data.projectId
                    );
                    const projectStatus = document.getElementById(
                        'project-status-' + data.projectId
                    );

                    if (projectCount) {{
                        projectCount.textContent = data.count;

                        const doel = Number(projectCount.dataset.target);
                        if (projectStatus) {{
                            projectStatus.innerHTML = data.count >= doel
                                ? `<span class='project-klaar'>Klaar!</span>`
                                : ``;
                        }}
                    }}
                }}

                if (data.confetti) {{
                    startConfetti();
                }}
            }} catch (error) {{
                console.error(error);
            }} finally {{
                button.disabled = false;
            }}
        }});
    }});
</script>

    
    </body>
    </html>";

    return html;
}

app.MapGet("/home", () => 
    Results.Content(@"
    <html>
    <body>
    <head>
    <title>User Dashboard</title>
    <link rel='stylesheet' href='/style.css'>
    </head>
    <div>
    <h2>Login Page</h2>
    <form method = 'post' action='/login'>
        <input name='email' placeholder='Email'/>
        <br/><br/>
        <input name='password'type='password' placeholder='Password'/>
        <br/><br/>
        <button type='submit'>Login</button>
    </form> 
    </div>
    </body>
    </html>
    ", "text/html")
);
app.MapPost("/verwijderen/{id:guid}", (Guid id) =>
{
    var project = projecten.FirstOrDefault(p => p.Id == id);

    if (project != null)
    {
        projecten.Remove(project);
    }

    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/activeren/{id:guid}", (Guid id) =>
{
    var gekozenProject = projecten.FirstOrDefault(p => p.Id == id);

    if (gekozenProject != null)
    {
        foreach (var project in projecten)
        {
            project.Actief = false;
        }

        gekozenProject.Actief = true;
    }

    return Results.Redirect("/");
}).DisableAntiforgery();


app.MapPost("/minus", () =>
{
    var actiefProject = projecten.FirstOrDefault(p => p.Actief);

    if (actiefProject != null &&
        actiefProject.HuidigeTelling > 0)
    {
        actiefProject.HuidigeTelling--;
    }

    int telling = actiefProject?.HuidigeTelling ?? 0;

    string status = actiefProject == null
        ? "Kies een project om te beginnen"
        : actiefProject.Afgerond
            ? "Project afgerond!"
            : $"Nog {actiefProject.Doelsteken - telling} steken te gaan";

    return Results.Json(new 
    { 
        count = telling,
        status = status,
        projectId = actiefProject?.Id,
        confetti = false
        });
}).DisableAntiforgery();



app.MapPost("/plus", () =>
{
    var actiefProject = projecten.FirstOrDefault(p => p.Actief);

    bool confetti = false;

    if (actiefProject != null &&
        actiefProject.HuidigeTelling < actiefProject.Doelsteken)
    {
        actiefProject.HuidigeTelling++;

        if (actiefProject.Afgerond)
        {
            confetti = true;
        }
    }

    int telling = actiefProject?.HuidigeTelling ?? 0;

    string status = actiefProject == null
        ? "Kies een project om te beginnen"
        : actiefProject.Afgerond
            ? "Project afgerond!"
            : $"Nog {actiefProject.Doelsteken - telling} steken te gaan";

    return Results.Json(new
    {
        count = telling,
        status = status,
        confetti = confetti,
        projectId = actiefProject?.Id
    });
}).DisableAntiforgery();


app.MapPost("/reset", () =>
{
    var actiefProject = projecten.FirstOrDefault(p => p.Actief);

    if (actiefProject != null)
    {
        actiefProject.HuidigeTelling = 0;
    }

    return Results.Redirect("/");
}).DisableAntiforgery();


app.MapGet("/", () =>
    Results.Content(RenderPage(), "text/html")
    );

app.MapGet("/users", ()=>
{
    string[] users = {"Tim","Henk","Jan"};

    string html = "<h2>User List</h2><ul>";

    foreach (var user in users)
        html += $"<li>{user}</li>";

    html += "</ul>";

    return Results.Content(html, "text/html");
});

app.MapGet("/toevoegen", () => 
    Results.Content(@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Nieuw project</title>
        <link rel='stylesheet' href='/style.css'>
    </head>
    <body>
        <div class='container'>
            <h1>Nieuw haakproject</h1>
            <p class='subtitle'>Voeg een nieuw project toe!</p>

            <form method='post' action='/toevoegen' class='toevoeg-form'>
                <input class='toevoeg-input' name='name' placeholder='Naam van je project' required>
                <input class='toevoeg-input' name='steken' type='number' min='1' placeholder='Aantal steken' required>
                <button class='toevoeg-button' type='submit'>Opslaan</button>
            </form>
            <br>

            <a href='/'>Terug naar mijn teller</a>
        </div>
    </body>
    </html>
    ", "text/html")
);

app.MapPost("/toevoegen", 
    ([Microsoft.AspNetCore.Mvc.FromForm] string name,
    [Microsoft.AspNetCore.Mvc.FromForm] string steken) =>
    {
        if (!string.IsNullOrWhiteSpace(name) && 
        int.TryParse(steken, out int stekentel) && stekentel > 0)
        {
            projecten.Add(new Project
            {
                Naam = name,
                Doelsteken = stekentel,
                HuidigeTelling = 0
            });
        }
    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/login",
    ([Microsoft.AspNetCore.Mvc.FromForm] string email,
    [Microsoft.AspNetCore.Mvc.FromForm] string password) =>
    {
        if (email == "admin@test.com" && password == "1234")
            return Results.Redirect("/dashboard");

        return Results.Content("Invalid Email or Password", "text/plain");
    })
    .DisableAntiforgery();
app.MapGet("/dashboard", () =>
    Results.Content("<h1>Welcome to your Dashboard!</h1>", "text/html")
);

app.Run();

class Project
{
    public Guid Id {get; set;} = Guid.NewGuid();
    public string Naam {get; set;} = "";
    public int Doelsteken {get; set;}
    public int HuidigeTelling {get; set;}
    public bool Actief {get; set;}

    public bool Afgerond => Doelsteken > 0 && HuidigeTelling >= Doelsteken;
}