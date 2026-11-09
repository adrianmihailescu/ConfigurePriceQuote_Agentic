type AgentDefinition = {
  fileName: string
  name: string
  description: string
  delegatedAgents: string[]
}

const agentFiles = import.meta.glob<string>('../../.github/agents/*.agent.md', {
  eager: true,
  query: '?raw',
  import: 'default',
})

function readFrontmatterValue(frontmatter: string, key: string, fileName: string) {
  const match = frontmatter.match(new RegExp(`^${key}:\\s*(.+)$`, 'm'))
  if (!match) {
    throw new Error(`Agent definition "${fileName}" is missing "${key}" frontmatter.`)
  }

  const value = match[1].trim()
  return value.startsWith('"') && value.endsWith('"')
    ? JSON.parse(value) as string
    : value
}

function parseAgentDefinition([path, content]: [string, string]): AgentDefinition {
  const fileName = path.split('/').at(-1) ?? path
  const frontmatter = content.match(/^---\r?\n([\s\S]*?)\r?\n---/)?.[1]
  if (!frontmatter) {
    throw new Error(`Agent definition "${fileName}" has invalid frontmatter.`)
  }

  const agentsMatch = frontmatter.match(/^agents:\s*\[([^\]]*)\]\s*$/m)
  const delegatedAgents = agentsMatch
    ? agentsMatch[1].split(',').map((agent) => agent.trim()).filter(Boolean)
    : []

  return {
    fileName,
    name: readFrontmatterValue(frontmatter, 'name', fileName),
    description: readFrontmatterValue(frontmatter, 'description', fileName),
    delegatedAgents,
  }
}

const definitions = Object.entries(agentFiles).map(parseAgentDefinition)

if (definitions.length === 0) {
  throw new Error('No agent definitions were found in .github/agents.')
}

export const agents = [...definitions].sort((left, right) => left.name.localeCompare(right.name))

const orchestrator = definitions.find((agent) => agent.delegatedAgents.length > 0)
if (!orchestrator) {
  throw new Error('No orchestrator agent definition was found in .github/agents.')
}

export const workflowAgents = orchestrator.delegatedAgents.map((name) => {
  const agent = definitions.find((definition) => definition.name === name)
  if (!agent) {
    throw new Error(`Orchestrator references missing agent definition "${name}".`)
  }

  return agent
})
