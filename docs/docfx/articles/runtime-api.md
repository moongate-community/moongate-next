---
title: Runtime API
---

# Runtime API

Moongate exposes a small HTTP API from the server process.

## Version

```http
GET /api/version
```

Returns the server version metadata as JSON.

## Metrics

```http
GET /metrics
```

Returns the latest metrics snapshot in OpenMetrics text format.

## Map Images

```http
GET /api/maps/{mapId}.png
```

Returns a radar-colour PNG image for the requested UO map facet. The image is
generated lazily on the first request, then cached on disk.

Generated map images are stored under:

```text
<root>/cache/images/maps/{mapId}.png
```

Subsequent requests return the cached PNG without regenerating it. Delete the
cached file to force regeneration.

Supported standard map ids are:

| Map id | Facet |
|---:|---|
| `0` | Felucca |
| `1` | Trammel |
| `2` | Ilshenar |
| `3` | Malas |
| `4` | Tokuno |
| `5` | TerMur |

The endpoint returns `404 Not Found` when the map id is unknown or the required
UO map files are unavailable in the configured `uo.client_files_directory`.

## Item Images

```http
GET /api/items/{itemId}.png
```

Returns a cropped and padded PNG image for the requested UO item art id. The
route expects hexadecimal ids in `0x...` form, for example:

```http
GET /api/items/0x001.png
```

Generated item images are stored under:

```text
<root>/cache/images/items/0x001.png
```

The filename is normalized to at least three uppercase hexadecimal digits. For
larger ids, the filename expands naturally, such as `0x1000.png`.

The endpoint returns `400 Bad Request` when the id is not in `0x...` format and
`404 Not Found` when no art exists for the requested id.

```http
POST /api/items/build
```

Generates the full item-art cache by scanning every legal art id exposed by the
configured UO art index. Existing cached PNG files are reused. The response
contains the number of generated, cached, skipped, and failed entries.
