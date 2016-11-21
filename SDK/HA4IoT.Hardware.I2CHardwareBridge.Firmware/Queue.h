#include <Arduino.h>

#define INIT_SIZE 2

template<typename T> class Queue 
{
  public:
    Queue ();
    ~Queue ();
    void Enqueue (const T i);
    T Dequeue ();
    bool IsEmpty () const;
	bool IsFull() const;
    int Count () const;
    

  private:
    void ResizeContentBuffer (const int s);
    T * ContentBuffer;    
    int Size;       
    int ItemCount;       
    int Head;        
    int Tail;        
};


template<typename T> Queue<T>::Queue () 
{
  Size = 0;      
  ItemCount = 0;      
  Head = 0;       
  Tail = 0;       

  ContentBuffer = (T *) malloc (sizeof (T) * INIT_SIZE);

  Size = INIT_SIZE;
}

template<typename T> Queue<T>::~Queue () 
{
  free (ContentBuffer); 

  ContentBuffer = NULL; 

  Size = 0;       
  ItemCount = 0;     
  Head = 0;       
  Tail = 0;        
}


template<typename T> void Queue<T>::ResizeContentBuffer (const int s) 
{
  T * temp = (T *) malloc (sizeof (T) * s);

  for (int i = 0; i < ItemCount; i++)
  {
	  temp[i] = ContentBuffer[(Head + i) % Size];
  }

  free (ContentBuffer);
  ContentBuffer = temp;

  Head = 0; 
  Tail = ItemCount;
  Size = s;
}

template<typename T> void Queue<T>::Enqueue (const T i) 
{

	if (IsFull())
	{
		ResizeContentBuffer(Size * 2);
	}

    ContentBuffer[Tail++] = i;
  
	if (Tail == Size)
	{
		Tail = 0;
	}

    ItemCount++;
}

template<typename T> T Queue<T>::Dequeue () 
{

	if (IsEmpty())
	{
	    exit(0);
	}

	T item = ContentBuffer[Head++];

	ItemCount--;

	if (Head == Size)
	{
		Head = 0;
	}

	// Shrink if necessary
	if (!IsEmpty() && (ItemCount <= Size / 4))
	{
		ResizeContentBuffer(Size / 2);
	}
 
	return item;
}

template<typename T> bool Queue<T>::IsEmpty () const 
{
  return ItemCount == 0;
}

template<typename T> bool Queue<T>::IsFull () const 
{
  return ItemCount == Size;
}

template<typename T> int Queue<T>::Count () const 
{
  return ItemCount;
}