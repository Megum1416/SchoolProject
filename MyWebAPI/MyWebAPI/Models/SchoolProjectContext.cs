using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyWebAPI.Models;

public partial class SchoolProjectContext : DbContext
{
    public SchoolProjectContext()
    {
    }

    public SchoolProjectContext(DbContextOptions<SchoolProjectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FollowList> FollowList { get; set; }

    public virtual DbSet<Members> Members { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    public virtual DbSet<RePost> RePost { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FollowList>(entity =>
        {
            entity.HasKey(e => e.FollowID).HasName("PK__FollowLi__2CE8108E4C17E2FA");

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
                .HasConstraintName("FK__FollowLis__Membe__4F7CD00D");
        });

        modelBuilder.Entity<Members>(entity =>
        {
            entity.HasKey(e => e.MemberID).HasName("PK__Members__0CF04B38755FA4B1");

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
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
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
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostID).HasName("PK__Post__AA126038C7BB648A");

            entity.Property(e => e.PostID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.FollowID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MemberID)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PostTitle).HasMaxLength(30);

            entity.HasOne(d => d.Follow).WithMany(p => p.Post)
                .HasForeignKey(d => d.FollowID)
                .HasConstraintName("FK__Post__FollowID__5165187F");

            entity.HasOne(d => d.Member).WithMany(p => p.Post)
                .HasForeignKey(d => d.MemberID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Post__MemberID__5070F446");
        });

        modelBuilder.Entity<RePost>(entity =>
        {
            entity.HasKey(e => e.RePostID).HasName("PK__RePost__002AC4B9A323FCC9");

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
                .HasConstraintName("FK__RePost__PostID__52593CB8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
