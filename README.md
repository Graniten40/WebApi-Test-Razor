# GoodFriends – Razor + WebAPI Client

**Course assignment – .NET Fullstack**

## Overview

This project is a Razor Pages client application that consumes the GoodFriends WebAPI.
The application allows viewing, creating, updating, and deleting friends, pets, and quotes.

Focus areas:

* API integration via HttpClient
* DTO mapping
* Razor Pages architecture
* Validation and ModelState handling
* Relationship loading (Friends ↔ Pets ↔ Quotes)

---

## Architecture

### Client

* ASP.NET Core Razor Pages
* Service layer: `GoodFriendsApiClient`
* DTO-based communication with WebAPI
* TempData + PRG pattern for post actions

### API interaction

All data is retrieved via REST endpoints:

* `/api/Friends`
* `/api/Pets`
* `/api/Quotes`
* `/api/Overview`
* `/api/Admin`

The client does not access the database directly.

---

## Features implemented

### Friends

* List friends
* View details
* Edit friend information
* View address

### Pets

* Add pet to friend
* Delete pet
* Show pets per friend

### Quotes

* Add quote
* Delete quote
* Show quotes per friend

### Overview

* Friends per country
* Friends per city
* City drill-down with pets

---

## Technical challenges encountered

### Issue: Pets and quotes were not scoped per friend

Symptoms:

* Sometimes all pets/quotes appeared for all friends
* Sometimes none appeared

Root cause:

* The API returned relationship data in different shapes depending on endpoint and seed state:

  * `friendId`
  * `friendIds`
  * embedded `friends[]`
* Server-side filtering was therefore unreliable.

### Solution implemented

Client-side filtering added in `GoodFriendsApiClient`:

Quotes and pets are matched against:

* `friendId`
* `friendIds`
* embedded `friends`

Additionally:

* Relations are explicitly loaded in `GetFriendDetailsAsync()`
* Pets and quotes are fetched separately and mapped per friend

Result:

* Correct relation mapping
* Stable rendering of Friend Details
* No dependency on inconsistent API payload structures

---

## Validation handling

The page contains multiple forms:

* Add Pet
* Add Quote

ModelState cleanup is used to ensure only the active form is validated:

* Quote keys removed when validating Pet
* Pet keys removed when validating Quote

This prevents cross-form validation errors.

---

## Error handling

Centralized HTTP error handling:

* `EnsureSuccessWithBodyAsync`
* Returns status + body for debugging

Validation responses (400):

* Parsed into field-level errors
* Displayed in UI

---

## Design decisions

* Avoid heavy `flat=false` queries where possible
* Load relations explicitly for stability
* Use DTO mapping instead of direct API models
* PRG pattern used for POST actions
* Client resilient to API payload variations

---

## Known limitations

* API contract is inconsistent regarding relationship structure
* Client-side filtering required as workaround
* Performance could improve if API enforced strict schema

---

## Future improvements

* Enforce consistent API relationship contract
* Introduce caching for overview endpoints
* Add pagination for quotes/pets
* Move filtering logic to API once schema stabilizes

---

## Running the project

### Requirements

* .NET SDK
* GoodFriends WebAPI running locally
* User Secrets configured

### Steps

1. Start WebAPI
2. Start Razor project
3. Navigate to:

   * `/Friends`
   * `/Overview`
   * `/Friends/Details/{id}`

---

## Author

Johan Persson
.NET Fullstack student
