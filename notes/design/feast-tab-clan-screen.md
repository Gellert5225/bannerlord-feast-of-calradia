# Feast Tab — Clan Screen

## Purpose
Player-facing UI for configuring a feast they want to host. Lives as
the fifth tab on the Clan screen, between "Fiefs" and "Other".

## Layout
Three-column grid mirroring other clan tabs:
- Left (160px): stage navigation list
- Center (flex): detail panel for selected stage
- Right (150px): finance summary + primary action button

## Stages (left column)
Three stages, each with a status icon:
- Venue — ✓ once a fief and scale are selected
- Guests — ⚠ once at least one is invited but cap not met,
           ✓ once any positive number invited (cap is a soft limit)
- Provisions — ✗ until all required items are in the host fief's
              inventory, then ✓

Status icons drive the gating of the "Send invitations" button:
all three must be ✓ for the button to be enabled.

## Center panel — Venue stage
[continues with each panel's contents, fields, behaviors]

## Center panel — Guests stage
[...]

## Center panel — Provisions stage
[...]

## Right panel — Finance & action
[...]

## State transitions
- Initial: Venue stage selected, all three panels stacked invisibly
  except Venue.
- Clicking a stage: that stage's panel becomes visible, others hidden.
  No animation in v1.
- Clicking "Send invitations" (when enabled): transitions the entire
  tab into a different mode — "Feast in progress" — covered in
  `ui-feast-tab-states.md` (TBD).

## Implementation notes
- Reuse the existing Finance panel component used by other clan tabs.
  Verify by reading the existing clan-screen XML before duplicating.
- The provisions panel reads inventory state from the host fief; it
  does not maintain its own state. When the player deposits into the
  fief inventory via the standard inventory screen, the provisions
  panel reflects this on next display.

## Open decisions
- Whether to add a "Take from party" quick-deposit button next to
  missing provisions (deferred — needs UX call)
- Whether RSVP probability is shown as percentage or coarser
  pill ("likely / uncertain / unlikely") — currently percentage
