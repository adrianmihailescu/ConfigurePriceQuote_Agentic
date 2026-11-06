import { useEffect, useMemo, useState, type FormEvent } from 'react'
import {
  Activity,
  ArrowDownRight,
  ArrowRight,
  BadgeCheck,
  Bell,
  Boxes,
  Check,
  ChevronDown,
  CircleHelp,
  ClipboardList,
  Cpu,
  FileText,
  LayoutDashboard,
  LifeBuoy,
  PackageCheck,
  Plus,
  Search,
  Send,
  Settings2,
  ShieldCheck,
  Sparkles,
  Tag,
  X,
} from 'lucide-react'
import { api } from './api'
import type { AgentStep, ChatResponse, Product, QuoteSummary } from './types'

const fallbackProducts: Product[] = [
  {
    id: 'edge-server-r440',
    name: 'Edge Server R440',
    category: 'Compute',
    description: 'Compact performance for demanding edge workloads.',
    basePrice: 4890,
    imageUrl: 'https://images.unsplash.com/photo-1597852074816-d933c7d2b988?auto=format&fit=crop&w=960&q=85',
    options: [
      { key: 'memory', label: 'Memory', required: true, choices: [{ id: '32gb', label: '32 GB', priceDelta: 0 }, { id: '64gb', label: '64 GB', priceDelta: 460 }, { id: '128gb', label: '128 GB', priceDelta: 1120 }] },
      { key: 'storage', label: 'Primary storage', required: true, choices: [{ id: '1tb-nvme', label: '1 TB NVMe', priceDelta: 0 }, { id: '2tb-nvme', label: '2 TB NVMe', priceDelta: 290 }, { id: '4tb-nvme', label: '4 TB NVMe', priceDelta: 740 }] },
      { key: 'support', label: 'Support', required: true, choices: [{ id: 'standard', label: 'Standard · 1 year', priceDelta: 0 }, { id: 'premium', label: 'Premium · 3 years', priceDelta: 620 }] },
    ],
  },
  {
    id: 'studio-workstation-x',
    name: 'Studio Workstation X',
    category: 'Workstation',
    description: 'A powerful, quiet workstation for creative teams.',
    basePrice: 3250,
    imageUrl: 'https://images.unsplash.com/photo-1587202372775-e229f172b9d7?auto=format&fit=crop&w=960&q=85',
    options: [
      { key: 'memory', label: 'Memory', required: true, choices: [{ id: '32gb', label: '32 GB', priceDelta: 0 }, { id: '64gb', label: '64 GB', priceDelta: 380 }] },
      { key: 'graphics', label: 'Graphics', required: true, choices: [{ id: 'pro-8gb', label: 'Pro · 8 GB', priceDelta: 0 }, { id: 'pro-16gb', label: 'Pro · 16 GB', priceDelta: 890 }] },
      { key: 'support', label: 'Support', required: true, choices: [{ id: 'standard', label: 'Standard · 1 year', priceDelta: 0 }, { id: 'premium', label: 'Premium · 3 years', priceDelta: 420 }] },
    ],
  },
  {
    id: 'atlas-storage-24',
    name: 'Atlas Storage 24',
    category: 'Storage',
    description: 'Scalable all-flash storage that grows with your team.',
    basePrice: 6120,
    imageUrl: 'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?auto=format&fit=crop&w=960&q=85',
    options: [
      { key: 'capacity', label: 'Usable capacity', required: true, choices: [{ id: '12tb', label: '12 TB', priceDelta: 0 }, { id: '24tb', label: '24 TB', priceDelta: 1380 }, { id: '48tb', label: '48 TB', priceDelta: 2980 }] },
      { key: 'network', label: 'Network', required: true, choices: [{ id: '10gbe', label: '10 GbE', priceDelta: 0 }, { id: '25gbe', label: '25 GbE', priceDelta: 540 }] },
      { key: 'support', label: 'Support', required: true, choices: [{ id: 'standard', label: 'Standard · 1 year', priceDelta: 0 }, { id: 'premium', label: 'Premium · 3 years', priceDelta: 760 }] },
    ],
  },
]

const seedQuotes: QuoteSummary[] = [
  { id: 1, quoteNumber: 'QT-2025-1048', customerName: 'Meridian Health', customerEmail: 'purchasing@meridian.example', productName: 'Edge Server R440', total: 5671.5, status: 'In review', createdAt: '2025-02-14T10:00:00Z' },
  { id: 2, quoteNumber: 'QT-2025-1047', customerName: 'Arcwell Design', customerEmail: 'it@arcwell.example', productName: 'Studio Workstation X', total: 3825, status: 'Sent', createdAt: '2025-02-13T10:00:00Z' },
  { id: 3, quoteNumber: 'QT-2025-1046', customerName: 'Summit Labs', customerEmail: 'ops@summit.example', productName: 'Atlas Storage 24', total: 7580, status: 'Approved', createdAt: '2025-02-12T10:00:00Z' },
]

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })
const exactCurrency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function initialOptions(product: Product) {
  return Object.fromEntries(product.options.map((group) => [group.key, group.choices[0]?.id ?? '']))
}

function App() {
  const [products, setProducts] = useState(fallbackProducts)
  const [quotes, setQuotes] = useState(seedQuotes)
  const [selectedProductId, setSelectedProductId] = useState(fallbackProducts[0].id)
  const [selectedOptions, setSelectedOptions] = useState<Record<string, string>>(() => initialOptions(fallbackProducts[0]))
  const [apiConnected, setApiConnected] = useState(false)
  const [chatText, setChatText] = useState('')
  const [chatResponse, setChatResponse] = useState<ChatResponse | null>(null)
  const [chatBusy, setChatBusy] = useState(false)
  const [quoteBusy, setQuoteBusy] = useState(false)
  const [quoteMessage, setQuoteMessage] = useState('')
  const [customerName, setCustomerName] = useState('')
  const [customerEmail, setCustomerEmail] = useState('')
  const [quoteDialogOpen, setQuoteDialogOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [activeNav, setActiveNav] = useState('Overview')

  useEffect(() => {
    let active = true
    void Promise.all([api.catalog(), api.quotes()])
      .then(([catalog, recentQuotes]) => {
        if (!active) return
        if (catalog.length > 0) {
          setProducts(catalog)
          setSelectedProductId(catalog[0].id)
          setSelectedOptions(initialOptions(catalog[0]))
        }
        setQuotes(recentQuotes)
        setApiConnected(true)
      })
      .catch(() => {
        if (active) setApiConnected(false)
      })
    return () => { active = false }
  }, [])

  const selectedProduct = products.find((product) => product.id === selectedProductId) ?? products[0]
  const filteredProducts = useMemo(
    () => products.filter((product) => `${product.name} ${product.category}`.toLowerCase().includes(query.toLowerCase())),
    [products, query],
  )

  const subtotal = selectedProduct.basePrice + selectedProduct.options.reduce((sum, group) => {
    const choice = group.choices.find((item) => item.id === selectedOptions[group.key])
    return sum + (choice?.priceDelta ?? 0)
  }, 0)
  const discount = subtotal >= 5000 ? subtotal * 0.05 : 0
  const total = subtotal - discount

  function chooseProduct(product: Product) {
    setSelectedProductId(product.id)
    setSelectedOptions(initialOptions(product))
    setChatResponse(null)
    setQuoteMessage('')
  }

  function changeOption(key: string, value: string) {
    setSelectedOptions((current) => ({ ...current, [key]: value }))
    setChatResponse(null)
  }

  async function askAgents(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setChatBusy(true)
    setChatResponse(null)
    try {
      const response = await api.chat(selectedProduct.id, selectedOptions, chatText)
      setChatResponse(response)
    } catch (error) {
      const fallbackSteps: AgentStep[] = [
        { agent: 'Product Agent', status: 'complete', summary: `Matched ${selectedProduct.name} in the sample catalog.` },
        { agent: 'Validation Agent', status: 'complete', summary: 'All selected sample options are available.' },
        { agent: 'Pricing Agent', status: 'complete', summary: `Sample estimate: ${exactCurrency.format(total)} USD.` },
        { agent: 'Quotation Agent', status: 'ready', summary: 'Ready to create a quote when the API and database are connected.' },
      ]
      setChatResponse({
        reply: `Sample workflow preview: ${error instanceof Error ? error.message : 'The API is unavailable.'} Your current sample configuration is ready to review.`,
        isValid: true,
        subtotal,
        discount,
        total,
        issues: [],
        agentSteps: fallbackSteps,
      })
    } finally {
      setChatBusy(false)
    }
  }

  async function createQuote(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setQuoteBusy(true)
    setQuoteMessage('')
    try {
      const quote = await api.createQuote({
        customerName,
        customerEmail,
        productId: selectedProduct.id,
        selectedOptions,
        notes: chatText,
      })
      setQuotes((current) => [quote, ...current])
      setQuoteMessage(`${quote.quoteNumber} created successfully.`)
      setQuoteDialogOpen(false)
      setCustomerName('')
      setCustomerEmail('')
    } catch (error) {
      setQuoteMessage(error instanceof Error ? error.message : 'Quote creation failed.')
    } finally {
      setQuoteBusy(false)
    }
  }

  const header = (
    <header className="topbar">
      <a className="brand" href="#" onClick={() => setActiveNav('Overview')} aria-label="My Personal Company home">
        <span className="brand-symbol"><Boxes size={17} aria-hidden="true" /></span>
        <span>acme<span className="brand-dot">.</span></span>
      </a>
      <div className="topbar-search">
        <Search size={16} aria-hidden="true" />
        <input aria-label="Search products" placeholder="Search products, quotes, customers..." value={query} onChange={(event) => setQuery(event.target.value)} />
        <kbd>⌘ K</kbd>
      </div>
      <div className="topbar-actions">
        <span className={`connection ${apiConnected ? 'is-online' : ''}`} title={apiConnected ? 'API connected' : 'Showing sample data'}>
          <span />{apiConnected ? 'Connected' : 'Sample mode'}
        </span>
        <button className="icon-button notification-button" aria-label="Notifications"><Bell size={18} /><i /></button>
        <button className="profile-button" aria-label="Account menu">AM <ChevronDown size={14} /></button>
      </div>
    </header>
  )

  return (
    <div className="app-shell">
      {header}
      <aside className="sidebar">
        <div className="workspace-switcher">
          <span className="workspace-icon">N</span>
          <span><strong>My Personal Company</strong><small>Sales workspace</small></span>
          <ChevronDown size={15} />
        </div>
        <div className="nav-label">WORKSPACE</div>
        <nav aria-label="Workspace navigation">
          {[
            { name: 'Overview', icon: LayoutDashboard },
            { name: 'Products', icon: Boxes },
            { name: 'Quotes', icon: FileText, count: String(quotes.length) },
            { name: 'Customers', icon: ClipboardList },
          ].map(({ name, icon: Icon, count }) => (
            <button key={name} className={`nav-item ${activeNav === name ? 'active' : ''}`} onClick={() => setActiveNav(name)}>
              <Icon size={18} /><span>{name}</span>{count && <span className="nav-count">{count}</span>}
            </button>
          ))}
        </nav>
        <div className="nav-label agents-label">AI TEAM</div>
        <div className="agent-nav">
          <span className="agent-avatar product-avatar"><Boxes size={14} /></span><span>Product Agent</span><i className="agent-online" />
          <span className="agent-avatar pricing-avatar"><Cpu size={14} /></span><span>Pricing Agent</span><i className="agent-online" />
          <span className="agent-avatar validation-avatar"><ShieldCheck size={14} /></span><span>Validation Agent</span><i className="agent-online" />
          <span className="agent-avatar quote-avatar"><FileText size={14} /></span><span>Quotation Agent</span><i className="agent-online" />
        </div>
        <div className="sidebar-bottom">
          <button className="nav-item" onClick={() => setActiveNav('Settings')}><Settings2 size={18} /><span>Settings</span></button>
          <button className="nav-item" onClick={() => setActiveNav('Help center')}><LifeBuoy size={18} /><span>Help center</span></button>
          <div className="upgrade-card">
            <div className="upgrade-sparkle"><Sparkles size={16} /></div>
            <strong>Your sales, on autopilot.</strong>
            <p>Let your AI team handle the busywork while you close the deal.</p>
            <button onClick={() => setActiveNav('AI team')}>Explore AI team <ArrowRight size={14} /></button>
          </div>
          <div className="user-profile"><div className="user-avatar">AM</div><span><strong>Alex Morgan</strong><small>Sales manager</small></span><CircleHelp size={17} /></div>
        </div>
      </aside>

      <main className="main-content">
        <div className="page-heading">
          <div>
            <div className="breadcrumb">Workspace <span>/</span> {activeNav}</div>
            <h1>Good morning<span className="wave">✳</span></h1>
            <p>Here’s what’s happening with your sales pipeline today.</p>
          </div>
          <button className="primary-button" onClick={() => { setQuoteMessage(''); setQuoteDialogOpen(true) }}><Plus size={17} /> Create a quote</button>
        </div>

        <section className="stats-grid" aria-label="Sales overview">
          <article className="stat-card">
            <div className="stat-top"><span>Quotes this month</span><span className="stat-icon blue"><FileText size={17} /></span></div>
            <div className="stat-value">{String(quotes.length + 24).padStart(2, '0')}<span className="stat-trend positive"><ArrowDownRight size={14} /> 12.8%</span></div>
            <div className="stat-foot">vs. last month <span className="mini-bars"><i /><i /><i /><i /><i /><i /><i /></span></div>
          </article>
          <article className="stat-card">
            <div className="stat-top"><span>Pipeline value</span><span className="stat-icon violet"><Tag size={17} /></span></div>
            <div className="stat-value">$284,560<span className="stat-trend positive"><ArrowDownRight size={14} /> 8.2%</span></div>
            <div className="stat-foot">across 18 open quotes <span className="mini-bars violet-bars"><i /><i /><i /><i /><i /><i /><i /></span></div>
          </article>
          <article className="stat-card">
            <div className="stat-top"><span>Win rate</span><span className="stat-icon green"><BadgeCheck size={17} /></span></div>
            <div className="stat-value">68.4%<span className="stat-trend positive"><ArrowDownRight size={14} /> 4.6%</span></div>
            <div className="stat-foot">healthy sales momentum <span className="mini-bars green-bars"><i /><i /><i /><i /><i /><i /><i /></span></div>
          </article>
          <article className="stat-card agent-stat">
            <div className="stat-top"><span>AI agents active</span><span className="stat-icon cyan"><Activity size={17} /></span></div>
            <div className="stat-value">04<span className="stat-caption">working together</span></div>
            <div className="stat-foot"><span className="pulse-dot" /> All systems operational</div>
          </article>
        </section>

        <section className="configurator-section">
          <div className="section-title-row">
            <div><div className="eyebrow"><span className="eyebrow-icon"><Settings2 size={14} /></span> CONFIGURE & QUOTE</div><h2>Build the right solution</h2></div>
            <button className="text-button" onClick={() => setActiveNav('Products')}>View all products <ArrowRight size={15} /></button>
          </div>

          <div className="workspace-grid">
            <section className="catalog-panel panel">
              <div className="panel-heading">
                <div><h3>Product catalog</h3><span>{filteredProducts.length} solutions available</span></div>
                <button className="filter-button"><Settings2 size={15} /> Filter</button>
              </div>
              <div className="product-list">
                {filteredProducts.map((product) => (
                  <button
                    className={`product-card ${selectedProduct.id === product.id ? 'selected' : ''}`}
                    key={product.id}
                    onClick={() => chooseProduct(product)}
                    aria-pressed={selectedProduct.id === product.id}
                  >
                    <span className="product-image-wrap">
                      <img src={product.imageUrl} alt="" className="product-image" />
                      <span className="product-category">{product.category}</span>
                      {selectedProduct.id === product.id && <span className="selected-check"><Check size={13} /></span>}
                    </span>
                    <span className="product-info">
                      <span className="product-name">{product.name}</span>
                      <span className="product-description">{product.description}</span>
                      <span className="product-card-bottom"><span>From <strong>{currency.format(product.basePrice)}</strong></span><span className="product-link">Configure <ArrowRight size={13} /></span></span>
                    </span>
                  </button>
                ))}
                {filteredProducts.length === 0 && <div className="empty-search">No products match “{query}”.</div>}
              </div>
              <div className="catalog-footer"><span><PackageCheck size={15} /> Product data updated today</span><button aria-label="More catalog options"><ChevronDown size={16} /></button></div>
            </section>

            <section className="config-panel panel">
              <div className="panel-heading config-title">
                <div><div className="eyebrow small-eyebrow">YOUR CONFIGURATION</div><h3>{selectedProduct.name}</h3></div>
                <span className="ready-badge"><span /> Ready to configure</span>
              </div>
              <div className="config-product">
                <img src={selectedProduct.imageUrl} alt={`${selectedProduct.name} product`} />
                <div><span>{selectedProduct.category} series</span><strong>{selectedProduct.name}</strong><small>{selectedProduct.description}</small></div>
              </div>
              <div className="option-groups">
                {selectedProduct.options.map((group) => (
                  <fieldset className="option-group" key={group.key}>
                    <legend>{group.label}{group.required && <span className="required-mark"> *</span>}</legend>
                    <div className="option-choices">
                      {group.choices.map((choice) => (
                        <button
                          className={`option-choice ${selectedOptions[group.key] === choice.id ? 'chosen' : ''}`}
                          key={choice.id}
                          onClick={() => changeOption(group.key, choice.id)}
                          aria-pressed={selectedOptions[group.key] === choice.id}
                        >
                          <span>{choice.label}</span>
                          {choice.priceDelta > 0 && <small>+{currency.format(choice.priceDelta)}</small>}
                        </button>
                      ))}
                    </div>
                  </fieldset>
                ))}
              </div>
              <div className="price-summary">
                <div><span>Base price</span><strong>{exactCurrency.format(selectedProduct.basePrice)}</strong></div>
                <div><span>Selected options</span><strong>{exactCurrency.format(subtotal - selectedProduct.basePrice)}</strong></div>
                {discount > 0 && <div className="discount-line"><span>Volume discount · 5%</span><strong>−{exactCurrency.format(discount)}</strong></div>}
                <div className="total-line"><span>Estimated total <small>USD</small></span><strong>{exactCurrency.format(total)}</strong></div>
              </div>
              <button className="primary-button full-button" onClick={() => { setQuoteMessage(''); setQuoteDialogOpen(true) }}>Continue to quote <ArrowRight size={16} /></button>
              <div className="estimate-note"><ShieldCheck size={14} /> Pricing is an estimate until your quote is reviewed.</div>
            </section>
          </div>
        </section>

        <section className="lower-grid">
          <section className="agent-panel panel">
            <div className="panel-heading">
              <div><div className="eyebrow"><span className="eyebrow-icon purple-icon"><Sparkles size={13} /></span> AGENTIC WORKFLOW</div><h3>Your AI team is ready</h3></div>
              <span className="live-tag"><i /> LIVE</span>
            </div>
            <div className="workflow">
              {[
                { name: 'Product Agent', note: 'Finds the right fit', Icon: Boxes, style: 'product-avatar' },
                { name: 'Validation Agent', note: 'Checks compatibility', Icon: ShieldCheck, style: 'validation-avatar' },
                { name: 'Pricing Agent', note: 'Calculates your estimate', Icon: Cpu, style: 'pricing-avatar' },
                { name: 'Quotation Agent', note: 'Prepares the proposal', Icon: FileText, style: 'quote-avatar' },
              ].map(({ name, note, Icon, style }, index) => (
                <div className="workflow-step" key={name}>
                  <div className={`workflow-icon ${style}`}><Icon size={16} /></div>
                  <div className="workflow-copy"><strong>{name}</strong><span>{note}</span></div>
                  <span className="workflow-state">{index === 0 && chatResponse ? <Check size={14} /> : index === 0 ? 'Ready' : 'Waiting'}</span>
                  {index < 3 && <span className="workflow-connector" />}
                </div>
              ))}
            </div>
            <form className="agent-chat" onSubmit={askAgents}>
              <span className="chat-sparkle"><Sparkles size={15} /></span>
              <input aria-label="Ask the AI team" value={chatText} onChange={(event) => setChatText(event.target.value)} placeholder="Ask your AI team about this configuration..." />
              <button aria-label="Send to agents" type="submit" disabled={chatBusy}><Send size={16} /></button>
            </form>
            {chatBusy && <div className="agent-result" role="status">Agents are reviewing your configuration…</div>}
            {chatResponse && (
              <div className="agent-result" aria-live="polite">
                <div className="agent-result-heading"><Sparkles size={15} /><strong>{chatResponse.reply}</strong></div>
                {!apiConnected && <span className="sample-note">Sample preview only · connect the API and SQL Server to persist quotes.</span>}
                <div className="agent-step-list">
                  {chatResponse.agentSteps.map((step) => <span key={step.agent}><Check size={12} /> <strong>{step.agent}:</strong> {step.summary}</span>)}
                </div>
              </div>
            )}
          </section>

          <section className="quotes-panel panel">
            <div className="panel-heading quotes-heading">
              <div><div className="eyebrow"><span className="eyebrow-icon green-icon"><FileText size={13} /></span> RECENT ACTIVITY</div><h3>Recent quotes</h3></div>
              <button className="text-button" onClick={() => setActiveNav('Quotes')}>View all <ArrowRight size={15} /></button>
            </div>
            <div className="quote-table">
              <div className="quote-table-head"><span>QUOTE</span><span>CUSTOMER</span><span>AMOUNT</span><span>STATUS</span></div>
              {quotes.slice(0, 4).map((quote) => (
                <div className="quote-row" key={quote.id}>
                  <span className="quote-id">{quote.quoteNumber}</span>
                  <span className="quote-customer"><strong>{quote.customerName}</strong><small>{quote.productName}</small></span>
                  <span className="quote-amount">{currency.format(quote.total)}</span>
                  <span className={`quote-status ${quote.status.toLowerCase().replaceAll(' ', '-')}`}>{quote.status}</span>
                </div>
              ))}
              {quotes.length === 0 && <div className="empty-quotes">No quotes yet. Configure a product to get started.</div>}
            </div>
            {quoteMessage && <div className={`quote-feedback ${quoteMessage.includes('created successfully') ? 'success' : 'error'}`} role="status">{quoteMessage}<button aria-label="Dismiss message" onClick={() => setQuoteMessage('')}><X size={14} /></button></div>}
            <div className="quote-panel-footer"><span><Activity size={14} /> Updated just now</span><button aria-label="Quote activity details"><ArrowRight size={14} /></button></div>
          </section>
        </section>
        <footer className="page-footer"><span>© 2025 My Personal Company</span><span><span className="pulse-dot" /> All systems operational</span><button onClick={() => setActiveNav('Help center')}>Need help? <ArrowRight size={13} /></button></footer>
      </main>

      {quoteDialogOpen && (
        <div className="modal-backdrop" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) setQuoteDialogOpen(false) }}>
          <section className="quote-modal" role="dialog" aria-modal="true" aria-labelledby="quote-modal-title">
            <div className="modal-heading"><div><span className="eyebrow">NEW QUOTATION</span><h2 id="quote-modal-title">Who are we quoting?</h2></div><button className="icon-button" aria-label="Close dialog" onClick={() => setQuoteDialogOpen(false)}><X size={18} /></button></div>
            <p className="modal-description">We’ll prepare a draft for {selectedProduct.name} at an estimated {exactCurrency.format(total)}.</p>
            <form onSubmit={createQuote}>
              <label>Customer name<input required value={customerName} onChange={(event) => setCustomerName(event.target.value)} placeholder="e.g. Meridian Health" /></label>
              <label>Customer email<input required type="email" value={customerEmail} onChange={(event) => setCustomerEmail(event.target.value)} placeholder="name@company.com" /></label>
              {quoteMessage && <div className="modal-error" role="alert">{quoteMessage}</div>}
              {!apiConnected && <div className="modal-info"><CircleHelp size={15} /> API and SQL Server connection is required to save this quote.</div>}
              <div className="modal-actions"><button type="button" className="secondary-button" onClick={() => setQuoteDialogOpen(false)}>Cancel</button><button type="submit" className="primary-button" disabled={quoteBusy}>{quoteBusy ? 'Creating…' : 'Create draft quote'} <ArrowRight size={15} /></button></div>
            </form>
          </section>
        </div>
      )}
    </div>
  )
}

export default App
