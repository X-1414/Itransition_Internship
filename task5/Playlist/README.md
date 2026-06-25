# Vinyl & Static — Fake Music Catalog

A single-page web application that generates an endless, fully reproducible catalog of fake songs, artists, albums, cover art, lyrics, and synthesized audio.


### Catalog generation

* Deterministic song generation based on locale, seed, and record index
* Infinite catalog with reproducible results
* Multiple locales supported through JSON locale files
* Procedurally generated:

  * Song titles
  * Artist names
  * Album titles
  * Genres
  * Reviews
  * Like counts

### Table View

* Paginated table (20 records per page)
* Expandable song details
* Adjustable average likes parameter
* Deterministic regeneration when parameters change

### Gallery View

* Infinite scrolling gallery
* Procedurally generated cover artwork
* Overlay detail view
* Lazy-loaded images

### Song Details

* Generated album cover
* Generated review text
* Synthesized audio track
* Time-synchronized lyrics highlighting during playback

### Audio Generation

* Procedural song planning:

  * Keys and scales
  * Chord progressions
  * Melody generation
  * Bass, pad,piano and drum tracks
* WAV synthesis
* MP3 export via FFmpeg

### Cover Generation

* Deterministic cover art generation using SkiaSharp
* Randomized palettes
* Multiple visual patterns:

  * Concentric circles
  * Grid patterns
  * Diagonal stripes
  * Radial bursts
  * Polka dots

### Export

* Export visible songs as ZIP archive
* Includes generated MP3 files
* Supports exporting:
  * Current table page (20 songs)
  * Currently loaded gallery items (24 songs)

---

## Technology Stack

### Backend

* ASP.NET Core 8
* C#
* Bogus
* SkiaSharp
* FFMpegCore

### Frontend

* Vanilla JavaScript (ES Modules)
* HTML5
* CSS3

---

## Running Locally

### Requirements

* .NET 8 SDK
* FFmpeg binaries

### Run

```bash
dotnet restore
dotnet run
```

Open:

```text
http://localhost:5000    (or the URL displayed by ASP.NET Core)


