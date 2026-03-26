<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { notesApi, type Note } from '../api'

const router = useRouter()
const auth = useAuthStore()

// State
const notes = ref<Note[]>([])
const loading = ref(false)
const search = ref('')
const sortBy = ref('newest')

// form is shared between create and edit — editingId tells us which mode we're in
const showForm = ref(false)
const editingId = ref<number | null>(null) // null = create, number = edit
const formTitle = ref('')
const formContent = ref('')
const formError = ref('')
const saving = ref(false)

// which note is open in the detail modal — null means it's closed
const selectedNote = ref<Note | null>(null)

// API calls
// called on load, on search input, and on sort change
async function fetchNotes() {
  loading.value = true
  try {
    const res = await notesApi.getAll(search.value, sortBy.value)
    notes.value = res.data
  } finally {
    loading.value = false
  }
}

async function saveNote() {
  // validate before hitting the API
  if (!formTitle.value.trim()) {
    formError.value = 'Title is required'
    return
  }
  saving.value = true
  formError.value = ''
  try {
    if (editingId.value) {
      await notesApi.update(editingId.value, {
        title: formTitle.value,
        content: formContent.value
      })
    } else {
      await notesApi.create({
        title: formTitle.value,
        content: formContent.value
      })
    }
    closeForm()
    await fetchNotes() // refresh the list so the changes show up
  } catch {
    formError.value = 'Failed to save note'
  } finally {
    saving.value = false
  }
}

async function deleteNote(id: number) {
  if (!confirm('Delete this note?')) return
  await notesApi.delete(id)
  // remove it locally instead of re-fetching the whole list
  notes.value = notes.value.filter(n => n.id !== id)
  // close detail modal if the deleted note was open
  if (selectedNote.value?.id === id) selectedNote.value = null
}

// Form helpers
function openCreate() {
  editingId.value = null
  formTitle.value = ''
  formContent.value = ''
  formError.value = ''
  showForm.value = true
}

function openEdit(note: Note) {
  editingId.value = note.id
  formTitle.value = note.title
  formContent.value = note.content ?? ''
  formError.value = ''
  selectedNote.value = null // close detail modal first
  showForm.value = true
}

function closeForm() {
  showForm.value = false
  editingId.value = null
  formTitle.value = ''
  formContent.value = ''
  formError.value = ''
}

// Detail modal
function openDetail(note: Note) { selectedNote.value = note }
function closeDetail() { selectedNote.value = null }

// Auth
function logout() {
  auth.logout()
  router.push('/login')
}

// Helpers
function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-GB', {
    day: 'numeric', month: 'short', year: 'numeric'
  })
}

function formatDateTime(d: string) {
  return new Date(d).toLocaleString('en-GB', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

onMounted(fetchNotes)
</script>

<template>
  <div class="min-h-screen bg-gray-50">

    <!-- ── Navbar ── -->
    <div class="bg-white border-b border-gray-100 px-6 py-4 flex justify-between items-center">
      <div class="flex items-center gap-2">
        <!-- app icon next to the title -->
        <span class="material-symbols-outlined text-blue-600">note_stack</span>
        <h1 class="text-lg font-bold text-gray-800">My Notes</h1>
      </div>
      <button @click="logout" class="flex items-center gap-1 text-sm text-gray-400 hover:text-red-500 transition-colors">
        <span class="material-symbols-outlined text-[18px]">logout</span>
        Logout
      </button>
    </div>

    <div class="max-w-5xl mx-auto px-6 py-8">

      <!-- ── Search + Sort + New button ── -->
      <div class="flex flex-col sm:flex-row gap-3 mb-6">
        <!-- search icon inside the input wrapper -->
        <div class="relative flex-1">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-gray-300 text-[18px]">search</span>
          <!-- @input fires on every keystroke so results update as the user types -->
          <input
            v-model="search"
            @input="fetchNotes"
            placeholder="Search notes..."
            class="w-full border border-gray-200 rounded-lg pl-9 pr-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <!-- @change fires when the user picks a different option -->
        <select
          v-model="sortBy"
          @change="fetchNotes"
          class="border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="newest">Newest first</option>
          <option value="oldest">Oldest first</option>
          <option value="title">By title</option>
        </select>
        <button
          @click="openCreate"
          class="flex items-center justify-center gap-1 bg-blue-600 text-white rounded-lg px-5 py-2.5 text-sm font-medium hover:bg-blue-700 transition-colors whitespace-nowrap"
        >
          <span class="material-symbols-outlined text-[18px]">add</span>
          New Note
        </button>
      </div>

      <!-- ── Loading ── -->
      <div v-if="loading" class="flex items-center justify-center gap-2 text-gray-400 py-16 text-sm">
        <span class="material-symbols-outlined text-[20px] animate-spin">progress_activity</span>
        Loading...
      </div>

      <!-- ── Empty state ── -->
      <div v-else-if="notes.length === 0" class="text-center text-gray-400 py-16">
        <!-- note icon replaces the emoji -->
        <span class="material-symbols-outlined text-6xl mb-3 block">note_stack</span>
        <p class="text-sm">No notes yet. Click <strong>+ New Note</strong> to get started.</p>
      </div>

      <!-- ── Notes grid ── -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="note in notes"
          :key="note.id"
          class="bg-white rounded-xl border border-gray-100 p-5 hover:shadow-md transition-shadow flex flex-col"
        >
          <!-- clicking the title opens the read-only detail view -->
          <h3
            @click="openDetail(note)"
            class="font-semibold text-gray-800 mb-2 line-clamp-1 cursor-pointer hover:text-blue-600 transition-colors"
          >
            {{ note.title }}
          </h3>
          <p class="text-gray-400 text-sm flex-1 line-clamp-3 mb-4">
            {{ note.content || 'No content' }}
          </p>
          <div class="flex justify-between items-center pt-3 border-t border-gray-50">
            <span class="text-xs text-gray-300">{{ formatDate(note.createdAt) }}</span>
            <div class="flex gap-3">
              <!-- icon-only action buttons with title for accessibility tooltip -->
              <button @click="openDetail(note)" title="View" class="text-gray-400 hover:text-gray-600">
                <span class="material-symbols-outlined text-[18px]">visibility</span>
              </button>
              <button @click="openEdit(note)" title="Edit" class="text-blue-400 hover:text-blue-600">
                <span class="material-symbols-outlined text-[18px]">edit</span>
              </button>
              <button @click="deleteNote(note.id)" title="Delete" class="text-red-400 hover:text-red-600">
                <span class="material-symbols-outlined text-[18px]">delete</span>
              </button>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- ── Detail modal ── -->
    <!-- @click.self closes the modal when clicking the dark backdrop, not the card -->
    <div
      v-if="selectedNote"
      class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 px-4"
      @click.self="closeDetail"
    >
      <div class="bg-white rounded-2xl w-full max-w-lg p-6">
        <div class="flex justify-between items-start mb-4">
          <h2 class="text-xl font-bold text-gray-800 flex-1 pr-4">{{ selectedNote.title }}</h2>
          <button @click="closeDetail" class="text-gray-300 hover:text-gray-500">
            <span class="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>

        <!-- whitespace-pre-wrap preserves line breaks from the textarea -->
        <p class="text-gray-600 text-sm leading-relaxed whitespace-pre-wrap min-h-[80px] mb-6">
          {{ selectedNote.content || 'No content' }}
        </p>

        <!-- timestamps show when the note was created and last edited -->
        <div class="border-t border-gray-100 pt-4 flex flex-col gap-1">
          <p class="text-xs text-gray-300 flex items-center gap-1">
            <span class="material-symbols-outlined text-[14px]">schedule</span>
            Created: {{ formatDateTime(selectedNote.createdAt) }}
          </p>
          <p class="text-xs text-gray-300 flex items-center gap-1">
            <span class="material-symbols-outlined text-[14px]">update</span>
            Updated: {{ formatDateTime(selectedNote.updatedAt) }}
          </p>
        </div>

        <div class="flex justify-end gap-3 mt-5">
          <button
            @click="deleteNote(selectedNote.id)"
            class="flex items-center gap-1 px-4 py-2 text-sm text-red-400 border border-red-100 rounded-lg hover:bg-red-50"
          >
            <span class="material-symbols-outlined text-[16px]">delete</span>
            Delete
          </button>
          <!-- openEdit closes this modal and opens the form -->
          <button
            @click="openEdit(selectedNote)"
            class="flex items-center gap-1 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <span class="material-symbols-outlined text-[16px]">edit</span>
            Edit
          </button>
        </div>
      </div>
    </div>

    <!-- ── Create / Edit modal ── -->
    <div
      v-if="showForm"
      class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 px-4"
      @click.self="closeForm"
    >
      <div class="bg-white rounded-2xl w-full max-w-lg p-6">
        <!-- title changes based on whether we're creating or editing -->
        <div class="flex items-center gap-2 mb-4">
          <span class="material-symbols-outlined text-blue-600">{{ editingId ? 'edit_note' : 'note_add' }}</span>
          <h2 class="text-lg font-bold text-gray-800">
            {{ editingId ? 'Edit Note' : 'New Note' }}
          </h2>
        </div>

        <div v-if="formError" class="flex items-center gap-1 bg-red-50 text-red-600 text-sm rounded-lg px-4 py-2 mb-3">
          <span class="material-symbols-outlined text-[16px]">error</span>
          {{ formError }}
        </div>

        <input
          v-model="formTitle"
          placeholder="Title *"
          class="w-full border border-gray-200 rounded-lg px-4 py-2.5 text-sm mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <!-- resize-none prevents the user from resizing the textarea and breaking the layout -->
        <textarea
          v-model="formContent"
          placeholder="Content (optional)"
          rows="5"
          class="w-full border border-gray-200 rounded-lg px-4 py-2.5 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
        />

        <div class="flex justify-end gap-3">
          <button
            @click="closeForm"
            class="flex items-center gap-1 px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
          >
            <span class="material-symbols-outlined text-[16px]">close</span>
            Cancel
          </button>
          <!-- disabled while saving to prevent double submits -->
          <button
            @click="saveNote"
            :disabled="saving"
            class="flex items-center gap-1 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            <span class="material-symbols-outlined text-[16px]">{{ saving ? 'hourglass_top' : 'save' }}</span>
            {{ saving ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>