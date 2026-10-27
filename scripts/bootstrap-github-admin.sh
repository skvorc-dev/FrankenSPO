#!/usr/bin/env bash
set -euo pipefail
# Requires GitHub CLI authenticated with repository administration permissions.
# For organization Projects creation, ensure gh auth has the project scope.
ORG="skvorc-dev"
gh auth status >/dev/null
for repo in FrankenSPO FrankenSPO.MiddleTier FrankenSPO.WebApi; do
  echo "== $ORG/$repo =="
  gh label create 'type:bug' --repo "$ORG/$repo" --color D73A4A --description 'Bug or regression' --force
  gh label create 'type:feature' --repo "$ORG/$repo" --color 7057FF --description 'Feature work' --force
  gh label create 'type:docs' --repo "$ORG/$repo" --color 0075CA --description 'Documentation and examples' --force
  gh label create 'type:security' --repo "$ORG/$repo" --color B60205 --description 'Security related' --force
  gh label create 'type:tests' --repo "$ORG/$repo" --color 0E8A16 --description 'Tests and conformance' --force
done
for priority in 'P0:Critical:BD1E1E' 'P1:High:EF6B2E' 'P2:Normal:FBCA04' 'P3:Low:0E8A16'; do
  IFS=: read -r p label color <<< "$priority"
  gh label create "priority:$p" --repo "$ORG/FrankenSPO" --color "$color" --description "$label priority" --force
done
for area in core providers query tracking security docs samples generator; do
  gh label create "area:$area" --repo "$ORG/FrankenSPO" --color 1D76DB --description "$area work" --force
done
create_milestone() {
  local repo="$1" title="$2" description="$3"
  if gh api "repos/$ORG/$repo/milestones?state=all" --paginate --jq '.[].title' | grep -Fxq "$title"; then
    echo "Milestone exists: $repo / $title"
  else
    gh api --method POST "repos/$ORG/$repo/milestones" -f title="$title" -f description="$description" >/dev/null
    echo "Created milestone: $repo / $title"
  fi
}
create_milestone FrankenSPO 'P0 — Foundation' 'Docs, secure contributor workflow, architecture, build and CI prerequisites'
create_milestone FrankenSPO 'P1 — SQLite PoC' 'Verified metadata, CRUD, basic queries, tracking, transaction safety and injection regressions'
create_milestone FrankenSPO 'P2 — Developer Preview' 'Versioned, compilable samples, packages, docs and capabilities'
create_milestone FrankenSPO 'P3 — PostgreSQL Conformance' 'Second provider, shared verified contract subset'
create_milestone FrankenSPO.MiddleTier 'M0 — Development Readiness' 'Begin implementation only after Core and remote protocol prerequisites pass'
create_milestone FrankenSPO.WebApi 'M0 — Development Readiness' 'Begin implementation only after model, CRUD, query and authorization prerequisites pass'
echo 'Labels and milestone definitions complete.'
echo 'Project board creation and repository rulesets still require verification in GitHub UI.'
