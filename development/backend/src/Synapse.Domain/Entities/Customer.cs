using Synapse.Domain.Common;

namespace Synapse.Domain.Entities;

/// <summary>顧客マスタ。出荷指示の出荷先として使用する。</summary>
public class Customer : Entity
{
    private Customer() { }

    /// <summary>顧客コード。ユニーク。</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>顧客名。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>住所。</summary>
    public string? Address { get; private set; }

    /// <summary>電話番号。</summary>
    public string? Phone { get; private set; }

    /// <summary>メールアドレス。</summary>
    public string? Email { get; private set; }

    /// <summary>アクティブフラグ。</summary>
    public bool IsActive { get; private set; } = true;

    public static Customer Create(string code, string name, string? address, string? phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("顧客コードは必須です。", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("顧客名は必須です。", nameof(name));

        return new Customer
        {
            Code     = code,
            Name     = name,
            Address  = address,
            Phone    = phone,
            Email    = email,
            IsActive = true,
        };
    }

    public void Update(string name, string? address, string? phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("顧客名は必須です。", nameof(name));

        Name    = name;
        Address = address;
        Phone   = phone;
        Email   = email;
    }

    public void Deactivate() => IsActive = false;
}
