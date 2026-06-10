# Item Template REST + Admin UI Design

Date: 2026-06-10

## Goal

Expose loaded item templates to administrators through a read-only REST API and an Admin UI catalog. The feature should let an administrator search, filter, inspect, and visually recognize item templates without editing YAML or runtime state.

## Current Context

Item templates are loaded at boot by `ItemTemplateYamlLoader` and stored in the in-memory `IItemTemplateService`. The registry already provides `GetAll()` and `TryGet(id)`, so the REST API should read from the registry instead of re-reading YAML files.

Item art is already exposed through `/api/items/{itemId}.png`. UO hue data is already available through `IHueStore`; hue entries contain 32 RGB colors and are 0-based internally, while template hue values use UO hue ids where `0` means no hue and `N` maps to `IHueStore.GetHue(N - 1)`.

## Scope

In scope:

- Admin-only, read-only item template REST endpoints.
- Server-side pagination, search, and filters.
- A reusable backend helper for list query normalization, text search, filtering, and pagination.
- Admin UI navigation entry for item templates.
- Dense table with item image, item metadata, filters, and search.
- Side detail panel for the selected template.
- React hue display component using real hue palette data from the backend.

Out of scope:

- Editing item templates from the UI.
- Writing back to YAML.
- Runtime reload of item templates.
- Global admin search across multiple domains.
- Export/import workflows.

## Backend API

All new item template metadata endpoints are admin-only and require an authenticated user with the `Administrator` role, matching the existing `/api/admin/users` convention.

### List Item Templates

`GET /api/admin/item-templates`

Query parameters:

- `page`: 1-based page number, normalized like existing paged endpoints.
- `pageSize`: bounded page size.
- `search`: free-text query.
- `tag`: exact tag filter, case-insensitive.
- `rarity`: optional `ItemRarity` filter.
- `layer`: optional `ItemLayerType` filter.
- `abstract`: optional boolean filter for `IsAbstract`.

Implementation note: `abstract` is the public query parameter name, but the C# handler should bind it through an explicit query name or manual request parsing because `abstract` is a language keyword.

Response:

- Existing `PagedResult<T>` style shape.
- Items are lightweight summaries for the table.

Summary DTO fields:

- `id`
- `name`
- `itemId`
- `itemIdHex`
- `imageUrl`
- `rarity`
- `layer`
- `tags`
- `isAbstract`
- `hue`

Search behavior:

- Case-insensitive.
- Matches `id`, `name`, `comment`, `tags`, `scriptId`.
- Matches `itemId` as decimal text.
- Matches `itemId` as normalized hex text such as `0x0F61` and `0xF61`.

Sorting:

- Stable default ordering by `id` ascending, case-insensitive.
- No user-controlled sort in the first iteration.

### Get Item Template Detail

`GET /api/admin/item-templates/{id}`

Behavior:

- Returns `404` when the template id is not found.
- Returns a full read-only DTO when found.

Detail DTO fields:

- All summary fields.
- `comment`
- `baseItem`
- `scriptId`
- `visibility`
- `amount`
- `weight`
- `hue`
- `isStackable`
- `isMovable`
- `gumpId`
- `params`

Params should preserve the item template parameter key, type, and value. Parameter key matching remains case-insensitive in the registry, but the API returns the loaded key names.

### Get Hue Detail

`GET /api/admin/hues/{hue}`

Behavior:

- `hue = 0` returns a known "none" descriptor with no colors.
- Valid non-zero hue `N` maps to `IHueStore.GetHue(N - 1)`.
- Missing out-of-range hue returns `404`.

Hue DTO fields:

- `value`
- `hex`
- `name`
- `isNone`
- `isKnown`
- `colors`

Each color entry contains:

- `index`
- `r`
- `g`
- `b`
- `hex`

Item template DTOs should include the same hue descriptor shape for their own `hue` field. If a template references an unknown hue, the template endpoint should still return successfully with `isKnown = false` and an empty color list.

## Generic List Query Helper

Add a small reusable backend helper instead of baking pagination and search logic into the item template endpoint.

Responsibilities:

- Normalize page and page size.
- Apply optional text search over caller-provided string fields.
- Apply caller-provided structured filters.
- Return a `PagedResult<T>` using the existing paged result shape.

Constraints:

- Keep it in-memory and enumerable-based for this feature because item templates live in an in-memory registry.
- Do not introduce a query DSL.
- Do not replace existing user pagination unless it becomes useful in a later refactor.

The item template endpoint will provide the searchable fields and filters. Future endpoints can reuse the helper when they have similar in-memory read models.

## UI Design

Add a new Admin navigation item: `Item Templates`.

The view uses layout A from brainstorming: dense table on the left, selected-template detail panel on the right.

### Toolbar

Controls:

- Search input with debounce.
- Tag filter.
- Rarity filter.
- Layer filter.
- Abstract filter.
- Refresh button.

The search and filters are server-side. Changing a search/filter resets the current page to 1.

### Table

Columns:

- Item image from `imageUrl`.
- `id`
- `name`
- `itemIdHex`
- `rarity`
- `layer`
- `tags`
- `isAbstract`
- `HueSwatch`

The item image must be visible directly in the table. If the image endpoint returns `404`, the UI shows a small neutral placeholder rather than a broken image.

Selecting a row loads or displays the detail panel. The table should remain usable while detail loading happens.

### Detail Panel

The detail panel shows:

- Larger item image preview.
- `name`
- `id`
- `comment`
- `baseItem`
- `scriptId`
- `visibility`
- `amount`
- `weight`
- `HueSwatch` with full palette.
- `isMovable`
- `isStackable`
- `gumpId`
- `tags`
- `params`

The panel starts with an empty selection state. It should not open a modal for normal inspection.

### HueSwatch Component

Create a React component for displaying hue values.

Table mode:

- Compact gradient strip when colors are available.
- Text value in hex, for example `0x021`.
- "None" chip for hue `0`.
- "Unknown" chip when the hue id is not in `hues.mul`.

Detail mode:

- 32-color palette when colors are available.
- Hue name when available.
- Decimal and hex values.
- Unknown and none states as explicit chips.

The component should not compute UO palettes on the client. It consumes the backend hue descriptor.

## Data Flow

1. Admin opens the Item Templates view.
2. UI calls `GET /api/admin/item-templates` with default pagination.
3. Backend reads from `IItemTemplateService.GetAll()`, applies filters/search, maps summaries, and returns a paged result.
4. Table renders image URLs and compact hue descriptors.
5. Admin selects a row.
6. UI calls `GET /api/admin/item-templates/{id}`.
7. Detail panel renders the full DTO, including params and full hue palette.

## Error Handling

Backend:

- Invalid enum filters return `400`.
- Missing template detail returns `404`.
- Missing hue endpoint detail returns `404`.
- List endpoint never fails solely because an item image or hue is missing.

Frontend:

- Unauthorized or forbidden API responses show the normal admin error state.
- Empty search results show an empty table state.
- Failed list load shows a retryable error.
- Failed detail load keeps the selected row visible and shows an error in the detail panel.
- Missing item images show placeholders.
- Unknown hues show the hue number plus an "Unknown hue" chip.

## Testing Strategy

Backend tests:

- List endpoint requires Administrator authorization.
- List endpoint returns paged summaries with image URLs and hue descriptors.
- Search matches `id`, `name`, `comment`, `tags`, `scriptId`, decimal `itemId`, and hex `itemId`.
- Filters apply for `tag`, `rarity`, `layer`, and `abstract`.
- Detail endpoint returns a full DTO for an existing template.
- Detail endpoint returns `404` for a missing template.
- Hue endpoint returns a none descriptor for `0`.
- Hue endpoint returns 32 RGB colors for a valid hue.
- Hue endpoint returns `404` for out-of-range hue ids.
- Generic list query helper covers pagination, search field matching, and filter composition.

Frontend verification:

- Build passes with `npm --prefix ui run build`.
- Manual browser check confirms Admin navigation, search, filters, row selection, image rendering, and hue swatches.

Final verification:

- `dotnet test`
- `npm --prefix ui run build`
- Pull request CI green.

## Acceptance Criteria

- Only administrators can access item template metadata endpoints.
- The item template list is paginated and searchable server-side.
- Search covers the approved generic text fields and item id formats.
- The UI shows item images in the table.
- The UI shows hue as a real palette-based React component, not just a raw number.
- Selecting a row shows the complete read-only template detail.
- No editing, YAML writes, or runtime reload behavior is introduced.
