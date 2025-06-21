using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class UserMail
{
    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public bool IsVerified { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
