﻿//******************************************************************************************************
//  FileBackedHashSet.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/02/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a lookup table backed by a file, with very little memory overhead.
    /// </summary>
    /// <typeparam name="T">The type of the items in the lookup table.</typeparam>
    public class FileBackedHashSet<T> : ISet<T>, IDisposable
    {
        #region [ Members ]

        // Fields
        private FileBackedLookupTable<T, object> m_lookupTable;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet()
            : this(Path.GetTempFileName(), EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(string filePath)
            : this(filePath, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="comparer">The equality comparer used to compare items in the hash set.</param>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(IEqualityComparer<T> comparer)
            : this(Path.GetTempFileName(), comparer)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements are copied to this hash set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="enumerable"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(IEnumerable<T> enumerable)
            : this(Path.GetTempFileName(), enumerable, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="comparer">The equality comparer used to compare items in the hash set.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(string filePath, IEqualityComparer<T> comparer)
        {
            m_lookupTable = new FileBackedLookupTable<T, object>(LookupTableType.HashSet, filePath, comparer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="enumerable">The enumerable whose elements are copied to this hash set.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null or <paramref name="enumerable"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(string filePath, IEnumerable<T> enumerable)
            : this(filePath, enumerable, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="enumerable">The enumerable whose elements are copied to this hash set.</param>
        /// <param name="comparer">The equality comparer used to compare items in the hash set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="enumerable"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
            : this(Path.GetTempFileName(), enumerable, comparer)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedHashSet{T}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="enumerable">The enumerable whose elements are copied to this hash set.</param>
        /// <param name="comparer">The equality comparer used to compare items in the hash set.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null or <paramref name="enumerable"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> cannot be serialized.</exception>
        public FileBackedHashSet(string filePath, IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
        {
            m_lookupTable = new FileBackedLookupTable<T, object>(LookupTableType.HashSet, filePath, comparer);

            foreach (T item in enumerable)
                Add(item);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the file backing this hash set.
        /// </summary>
        /// <exception cref="ArgumentException">FilePath is set and is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <remarks>
        /// Changes to this property will cause the file to close if the file is already opened.
        /// Data will not be automatically written from the old file to the new file.
        /// </remarks>
        public string FilePath
        {
            get
            {
                return m_lookupTable.FilePath;
            }
            set
            {
                m_lookupTable.FilePath = value;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return m_lookupTable.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return m_lookupTable.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the signature of the file backing the lookup table.
        /// </summary>
        /// <exception cref="ArgumentNullException">Attempt is made to set Signature to a null value.</exception>
        /// <exception cref="ArgumentException">Attempt is made to set Signature to a value larger than the maximum allowed size.</exception>
        /// <exception cref="NotSupportedException">Attempt is made to modify Signature of a read-only lookup table.</exception>
        public byte[] Signature
        {
            get
            {
                return m_lookupTable.Signature;
            }
            set
            {
                m_lookupTable.Signature = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the cache used
        /// to store data from the file in memory.
        /// </summary>
        public long CacheSize
        {
            get
            {
                return m_lookupTable.CacheSize;
            }
            set
            {
                m_lookupTable.CacheSize = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the file backing this hash set.
        /// </summary>
        /// <exception cref="InvalidOperationException">File is already open.</exception>
        public void Open()
        {
            m_lookupTable.Open();
        }

        /// <summary>
        /// Adds an element to the current set and returns a value to indicate if the element was successfully added. 
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        /// true if the element is added to the set; false if the element is already in the set.
        /// </returns>
        public bool Add(T item)
        {
            return m_lookupTable.TryAdd(item, null);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(T item)
        {
            return m_lookupTable.Remove(item);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return m_lookupTable.ContainsKey(item);
        }

        /// <summary>
        /// Modifies the current set so that it contains all elements that are present in either the current set or the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
                Add(item);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            m_lookupTable.UnmarkAll();

            foreach (T item in other)
                m_lookupTable.TryMark(item);

            m_lookupTable.RemoveUnmarked();
        }

        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
                m_lookupTable.Remove(item);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are present
        /// either in the current set or in the specified collection, but not both. 
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            List<T> list;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            list = new List<T>();
            m_lookupTable.UnmarkAll();

            foreach (T item in other)
            {
                if (!m_lookupTable.TryMark(item))
                    list.Add(item);
            }

            m_lookupTable.RemoveMarked();

            foreach (T item in list)
                Add(item);
        }

        /// <summary>
        /// Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a subset of <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            m_lookupTable.UnmarkAll();

            foreach (T item in other)
                m_lookupTable.TryMark(item);

            return m_lookupTable.AllMarked();
        }

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a superset of <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
            {
                if (!Contains(item))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. </param>
        /// <returns>
        /// true if the current set is a proper superset of <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            m_lookupTable.UnmarkAll();

            foreach (T item in other)
            {
                if (!m_lookupTable.TryMark(item))
                    return false;
            }

            return !m_lookupTable.AllMarked();
        }

        /// <summary>
        /// Determines whether the current set is a proper (strict) subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a proper subset of <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            bool canBeProperSubset;

            if ((object)other == null)
                throw new ArgumentNullException("other");
            
            canBeProperSubset = false;
            m_lookupTable.UnmarkAll();

            foreach (T item in other)
            {
                if (!m_lookupTable.TryMark(item))
                    canBeProperSubset = true;
            }

            return canBeProperSubset && m_lookupTable.AllMarked();
        }

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set and <paramref name="other"/> share at least one common element; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool Overlaps(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
            {
                if (Contains(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is equal to <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool SetEquals(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            m_lookupTable.UnmarkAll();

            foreach (T item in other)
            {
                if (!m_lookupTable.TryMark(item))
                    return false;
            }

            return m_lookupTable.AllMarked();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Clear()
        {
            m_lookupTable.Clear();
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((object)array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if (m_lookupTable.Count > array.Length - arrayIndex)
                throw new ArgumentException("Not enough available space in array to copy elements from hash set");

            foreach (T item in this)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return m_lookupTable.GetKeys().GetEnumerator();
        }

        /// <summary>
        /// Defragments the item section of the hash set,
        /// which gets fragmented after removing items.
        /// </summary>
        public void Compact()
        {
            m_lookupTable.Compact();
        }

        /// <summary>
        /// Closes the file backing this hash set.
        /// </summary>
        public void Close()
        {
            m_lookupTable.Close();
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="FileBackedHashSet{T}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileBackedHashSet{T}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Close();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
