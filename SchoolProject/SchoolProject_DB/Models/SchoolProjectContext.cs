using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SchoolProject_DB.Models;

public partial class SchoolProjectContext : DbContext
{
    public SchoolProjectContext(DbContextOptions<SchoolProjectContext> options)
        : base(options)
    {
    }

    //public SchoolProjectContext()
    //{
    //}

    //1.2.3 在dbStudentsContext.cs裡撰寫連線到資料庫的程式
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //        => optionsBuilder.UseSqlServer("Data Source=MCSDD11301-2;Database=SchoolProject;TrustServerCertificate=True;User ID=sp;Password=123456");

    public virtual DbSet<FollowList> FollowList { get; set; }

    public virtual DbSet<Members> Members { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    public virtual DbSet<RePost> RePost { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FollowList>(entity =>
        {
            entity.HasKey(e => e.FollowID).HasName("PK__FollowLi__2CE8108E3524F689");

            entity.Property(e => e.FollowID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LodestoneID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MemberID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.FollowList)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FollowLis__Membe__3D5E1FD2");
        });

        modelBuilder.Entity<Members>(entity =>
        {
            entity.HasKey(e => e.MemberID).HasName("PK__Members__0CF04B38DC4F050F");

            entity.Property(e => e.MemberID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DataCenter)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FamilyName)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FirstName)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LodestoneID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Password)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ServerName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(30);

            // 新增 IsAdmin 屬性的映射
            entity.Property(e => e.IsAdmin)
                .HasColumnName("IsAdmin")
                .HasColumnType("bit")
                .HasDefaultValue(false); // 設定預設值為 false
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostID).HasName("PK__Post__AA126038668B090E");

            entity.Property(e => e.PostID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.Property(e => e.Description).HasMaxLength(250);

            // 已移除 FollowID 欄位及其設定

            entity.Property(e => e.MemberID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();

            entity.Property(e => e.PostTitle).HasMaxLength(30);

            // 已移除與 Follow 表的外鍵關聯

            entity.HasOne(d => d.Member).WithMany(p => p.Post)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Post__MemberID__3E52440B");
        });


        modelBuilder.Entity<RePost>(entity =>
        {
            entity.HasKey(e => e.RePostID).HasName("PK__RePost__002AC4B94E1A15EB");

            entity.Property(e => e.RePostID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.PostID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Post).WithMany(p => p.RePost)
                .HasForeignKey(d => d.PostID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RePost__PostID__403A8C7D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
