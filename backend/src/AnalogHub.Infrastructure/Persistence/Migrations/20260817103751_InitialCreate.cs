using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace AnalogHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "Gear",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MountType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AcquiredOn = table.Column<DateOnly>(type: "date", nullable: true),
                    GearType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    SupportedFormats = table.Column<int[]>(type: "integer[]", nullable: true),
                    GuideNumber = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: true),
                    HasTtl = table.Column<bool>(type: "boolean", nullable: true),
                    FocalLengthMinMm = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    FocalLengthMaxMm = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    MaxAperture = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    MinAperture = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gear", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeChunks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilmRolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Format = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NominalIso = table.Column<int>(type: "integer", nullable: false),
                    ExposedAtIso = table.Column<int>(type: "integer", nullable: true),
                    FrameCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CameraBodyId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateLoaded = table.Column<DateOnly>(type: "date", nullable: true),
                    DateFinished = table.Column<DateOnly>(type: "date", nullable: true),
                    DateDeveloped = table.Column<DateOnly>(type: "date", nullable: true),
                    LabName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DeveloperNotes = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmRolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilmRolls_Gear_CameraBodyId",
                        column: x => x.CameraBodyId,
                        principalTable: "Gear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FilmRollId = table.Column<Guid>(type: "uuid", nullable: false),
                    CameraBodyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LensId = table.Column<Guid>(type: "uuid", nullable: true),
                    FlashId = table.Column<Guid>(type: "uuid", nullable: true),
                    FrameNumber = table.Column<int>(type: "integer", nullable: true),
                    CaptureDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OriginalStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreviewStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BlurHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    WidthPx = table.Column<int>(type: "integer", nullable: true),
                    HeightPx = table.Column<int>(type: "integer", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<byte>(type: "smallint", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProcessingError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Exif_Aperture = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Exif_ShutterSpeed = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Exif_IsoUsed = table.Column<int>(type: "integer", nullable: true),
                    Exif_FocalLengthMm = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    Exif_FlashFired = table.Column<bool>(type: "boolean", nullable: true),
                    Exif_MeteringMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Exif_ScannerModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Exif_GpsLatitude = table.Column<double>(type: "double precision", nullable: true),
                    Exif_GpsLongitude = table.Column<double>(type: "double precision", nullable: true),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.CheckConstraint("CK_Photos_Rating_Range", "\"Rating\" >= 0 AND \"Rating\" <= 5");
                    table.ForeignKey(
                        name: "FK_Photos_FilmRolls_FilmRollId",
                        column: x => x.FilmRollId,
                        principalTable: "FilmRolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Photos_Gear_CameraBodyId",
                        column: x => x.CameraBodyId,
                        principalTable: "Gear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Photos_Gear_FlashId",
                        column: x => x.FlashId,
                        principalTable: "Gear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Photos_Gear_LensId",
                        column: x => x.LensId,
                        principalTable: "Gear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CoverPhotoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Photos_CoverPhotoId",
                        column: x => x.CoverPhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PhotoAiCritiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompositionScore = table.Column<int>(type: "integer", nullable: true),
                    CompositionNotes = table.Column<string>(type: "text", nullable: true),
                    LightingNotes = table.Column<string>(type: "text", nullable: true),
                    PosingNotes = table.Column<string>(type: "text", nullable: true),
                    RecommendationsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SuggestedTagsJson = table.Column<string>(type: "jsonb", nullable: false),
                    RawResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoAiCritiques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotoAiCritiques_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhotoTags",
                columns: table => new
                {
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAiSuggested = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoTags", x => new { x.PhotoId, x.TagId });
                    table.ForeignKey(
                        name: "FK_PhotoTags_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhotoTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumPhotos",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumPhotos", x => new { x.AlbumId, x.PhotoId });
                    table.ForeignKey(
                        name: "FK_AlbumPhotos_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumPhotos_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhotos_AlbumId_SortOrder",
                table: "AlbumPhotos",
                columns: new[] { "AlbumId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhotos_PhotoId",
                table: "AlbumPhotos",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CoverPhotoId",
                table: "Albums",
                column: "CoverPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmRolls_CameraBodyId",
                table: "FilmRolls",
                column: "CameraBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmRolls_Status",
                table: "FilmRolls",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Gear_Brand_Model",
                table: "Gear",
                columns: new[] { "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeChunks_DocumentTitle_ChunkIndex",
                table: "KnowledgeChunks",
                columns: new[] { "DocumentTitle", "ChunkIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeChunks_Embedding",
                table: "KnowledgeChunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoAiCritiques_PhotoId",
                table: "PhotoAiCritiques",
                column: "PhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_CameraBodyId",
                table: "Photos",
                column: "CameraBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_CaptureDateUtc",
                table: "Photos",
                column: "CaptureDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_Embedding",
                table: "Photos",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_FilmRollId",
                table: "Photos",
                column: "FilmRollId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_FlashId",
                table: "Photos",
                column: "FlashId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_LensId",
                table: "Photos",
                column: "LensId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ProcessingStatus",
                table: "Photos",
                column: "ProcessingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoTags_TagId",
                table: "PhotoTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumPhotos");

            migrationBuilder.DropTable(
                name: "KnowledgeChunks");

            migrationBuilder.DropTable(
                name: "PhotoAiCritiques");

            migrationBuilder.DropTable(
                name: "PhotoTags");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "FilmRolls");

            migrationBuilder.DropTable(
                name: "Gear");
        }
    }
}
